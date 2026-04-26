using AI.Application.Features.Post.DTOs;
using Marketing.Application.Services;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.Post.Queries.GetThreadsMetrics;

public sealed class GetThreadsMetricsQueryHandler
    : IQueryHandler<GetThreadsMetricsQuery, ThreadsMetricsDto>
{
    private readonly IThreadsMetricsService _threadsMetricsService;
    private readonly IExternalDistribuitionRepository _externalDistributionRepository;

    public GetThreadsMetricsQueryHandler(
        IThreadsMetricsService threadsMetricsService,
        IExternalDistribuitionRepository externalDistributionRepository)
    {
        _threadsMetricsService = threadsMetricsService;
        _externalDistributionRepository = externalDistributionRepository;
    }

    public async Task<Result<ThreadsMetricsDto>> Handle(
        GetThreadsMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var mediaId = query.MediaId;
        string? externalUrl = null;

        // Backward-compatible behavior:
        // if caller accidentally sends internal distribution GUID,
        // resolve it to Threads ExternalPostId before calling Threads API.
        if (Guid.TryParse(query.MediaId, out var distributionId))
        {
            var distribution = await _externalDistributionRepository.GetByIdAsync(distributionId, cancellationToken);

            if (distribution is null)
                return Result.Failure<ThreadsMetricsDto>(
                    MarketingErrors.ExternalDistribution.NotFound(distributionId));

            if (distribution.Platform != ExternalPlatform.Threads || !distribution.IsSent())
                return Result.Failure<ThreadsMetricsDto>(
                    MarketingErrors.Distribution.NotFound(ExternalPlatform.Threads));

            if (string.IsNullOrWhiteSpace(distribution.ExternalPostId))
                return Result.Failure<ThreadsMetricsDto>(
                    MarketingErrors.Distribution.ExternalPostIdMissing);

            mediaId = distribution.ExternalPostId;
            externalUrl = distribution.ExternalUrl;
        }

        var metrics = await _threadsMetricsService.GetMetricsAsync(
            mediaId,
            externalUrl,
            cancellationToken);

        if (metrics is null)
            return Result.Failure<ThreadsMetricsDto>(Error.Failure(
                "Threads.MetricsFetchFailed",
                "Failed to fetch Threads media metrics"));

        return Result.Success(metrics);
    }
}