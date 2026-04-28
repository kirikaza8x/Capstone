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

public sealed class GetFacebookMetricsQueryHandler
    : IQueryHandler<GetFacebookMetricsQuery, FacebookMetricsDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IFacebookMetricsService _facebookMetricsService;
    private readonly ITicketingPublicApi _ticketingPublicApi;

    public GetFacebookMetricsQueryHandler(
        IPostRepository postRepository,
        IFacebookMetricsService facebookMetricsService,
        ITicketingPublicApi ticketingPublicApi)
    {
        _postRepository = postRepository;
        _facebookMetricsService = facebookMetricsService;
        _ticketingPublicApi = ticketingPublicApi;
    }

    public async Task<Result<FacebookMetricsDto>> Handle(
        GetFacebookMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(
            query.PostId, cancellationToken);

        if (post is null)
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Post.NotFound(query.PostId));

        var distribution = post.ExternalDistributions
            .FirstOrDefault(d => d.Id == query.DistributionId
                              && d.Platform == ExternalPlatform.Facebook
                              && d.IsSent());

        if (distribution is null)
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Distribution.NotFound(ExternalPlatform.Facebook));

        if (string.IsNullOrWhiteSpace(distribution.ExternalPostId))
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Distribution.ExternalPostIdMissing);

        var metricsTask = _facebookMetricsService.GetMetricsAsync(
            distribution.ExternalPostId,
            distribution.ExternalUrl,
            cancellationToken);

        var ordersTask = _ticketingPublicApi.GetOrdersByEventIdAsync(
            post.EventId, cancellationToken);

        await Task.WhenAll(metricsTask, ordersTask);

        var metrics = await metricsTask;
        var orders = await ordersTask;

        if (metrics is null)
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        var ticketsSold = orders.Count;

        var totalEngagement = metrics.Likes + metrics.Comments + metrics.Shares + (int)metrics.Clicks;
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