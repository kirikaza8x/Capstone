using AI.Application.Features.Post.DTOs;
using Marketing.Application.Services;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.Post.Queries.GetThreadsMetrics;

public sealed class GetThreadsAccountMetricsQueryHandler
    : IQueryHandler<GetThreadsAccountMetricsQuery, ThreadsAccountMetricsDto>
{
    private readonly IThreadsMetricsService _threadsMetricsService;

    public GetThreadsAccountMetricsQueryHandler(IThreadsMetricsService threadsMetricsService)
    {
        _threadsMetricsService = threadsMetricsService;
    }

    public async Task<Result<ThreadsAccountMetricsDto>> Handle(
        GetThreadsAccountMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var metrics = await _threadsMetricsService.GetAccountTotalsAsync(
            query.Since,
            query.Until,
            cancellationToken);

        if (metrics is null)
            return Result.Failure<ThreadsAccountMetricsDto>(Error.Failure(
                "Threads.AccountMetricsFetchFailed",
                "Failed to fetch Threads account metrics"));

        return Result.Success(metrics);
    }
}