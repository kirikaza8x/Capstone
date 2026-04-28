using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Application.Services;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Marketing.Application.Posts.Handlers;

public sealed class GetInstagramMetricsQueryHandler
    : IQueryHandler<GetInstagramMetricsQuery, InstagramMetricsDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IInstagramMetricsService _instagramMetricsService;
    private readonly ITicketingPublicApi _ticketingPublicApi;

    public GetInstagramMetricsQueryHandler(
        IPostRepository postRepository,
        IInstagramMetricsService instagramMetricsService,
        ITicketingPublicApi ticketingPublicApi)
    {
        _postRepository = postRepository;
        _instagramMetricsService = instagramMetricsService;
        _ticketingPublicApi = ticketingPublicApi;
    }

    public async Task<Result<InstagramMetricsDto>> Handle(
        GetInstagramMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(
            query.PostId, cancellationToken);

        if (post is null)
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Post.NotFound(query.PostId));

        var distribution = post.ExternalDistributions
            .FirstOrDefault(d => d.Id == query.DistributionId
                              && d.Platform == ExternalPlatform.Instagram
                              && d.IsSent());

        if (distribution is null)
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Distribution.NotFound(ExternalPlatform.Instagram));

        if (string.IsNullOrWhiteSpace(distribution.ExternalPostId))
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Distribution.ExternalPostIdMissing);

        var metricsTask = _instagramMetricsService.GetMetricsAsync(
            distribution.ExternalPostId,
            distribution.ExternalUrl,
            cancellationToken);

        var ordersTask = _ticketingPublicApi.GetOrdersByEventIdAsync(
            post.EventId, cancellationToken);

        await Task.WhenAll(metricsTask, ordersTask);

        var metrics = await metricsTask;
        var orders = await ordersTask;

        if (metrics is null)
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        var ticketsSold = orders.Count;

        var totalEngagement = metrics.Likes + metrics.Comments 
            + (int)metrics.Shares + (int)metrics.Saves;

        var engagementRate = metrics.Reach > 0
            ? Math.Round((double)totalEngagement / metrics.Reach * 100, 2)
            : 0;

        int buyCount = distribution.BuyCount;
        int clickCount = distribution.ClickCount;

        var conversionRate = clickCount > 0
            ? Math.Round((double)buyCount / clickCount * 100, 2)
            : 0;

        return Result.Success(metrics with
        {
            BuyCount = buyCount,
            ClickCount = clickCount,
            TicketsSold = ticketsSold,
            ConversionRate = conversionRate,
            ConversionRateFormatted = $"{conversionRate}%",
            EngagementRate = engagementRate,
            EngagementRateFormatted = $"{engagementRate}%"
        });
    }
}