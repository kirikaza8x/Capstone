using AI.Application.Features.Post.DTOs;
using Marketing.Application.Services;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.Post.Queries.GetThreadsMetrics;

public sealed class GetThreadsMetricsByDistributionQueryHandler
    : IQueryHandler<GetThreadsMetricsByDistributionQuery, ThreadsMetricsDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IThreadsMetricsService _threadsMetricsService;

    public GetThreadsMetricsByDistributionQueryHandler(
        IPostRepository postRepository,
        IThreadsMetricsService threadsMetricsService)
    {
        _postRepository = postRepository;
        _threadsMetricsService = threadsMetricsService;
    }

    public async Task<Result<ThreadsMetricsDto>> Handle(
        GetThreadsMetricsByDistributionQuery query,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(
            query.PostId,
            cancellationToken);

        if (post is null)
            return Result.Failure<ThreadsMetricsDto>(
                MarketingErrors.Post.NotFound(query.PostId));

        var distribution = post.ExternalDistributions
            .FirstOrDefault(d => d.Id == query.DistributionId
                              && d.Platform == ExternalPlatform.Threads
                              && d.IsSent());

        if (distribution is null)
            return Result.Failure<ThreadsMetricsDto>(
                MarketingErrors.Distribution.NotFound(ExternalPlatform.Threads));

        if (string.IsNullOrWhiteSpace(distribution.ExternalPostId))
            return Result.Failure<ThreadsMetricsDto>(
                MarketingErrors.Distribution.ExternalPostIdMissing);

        if (distribution.ExternalPostId.Contains("REAL_THREADS_MEDIA_ID", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<ThreadsMetricsDto>(Error.Validation(
                "Distribution.InvalidExternalPostId",
                "Stored Threads ExternalPostId is a placeholder. Update webhook payload with the real Threads media id."));

        var metricsTask = _threadsMetricsService.GetMetricsAsync(
            distribution.ExternalPostId,
            distribution.ExternalUrl,
            cancellationToken);

        await metricsTask;

        var metrics = await metricsTask;

        if (metrics is null)
            return Result.Failure<ThreadsMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        var buyCount = distribution.BuyCount;
        var clickCount = distribution.ClickCount;

        var conversionRate = clickCount > 0
            ? Math.Round((double)buyCount / clickCount * 100, 2)
            : 0;

        return Result.Success(metrics with
        {
            BuyCount = buyCount,
            ClickCount = clickCount,
            ConversionRate = conversionRate,
            ConversionRateFormatted = $"{conversionRate}%"
        });
    }
}