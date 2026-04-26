using AI.Application.Features.Post.DTOs;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Post.Queries.GetThreadsMetrics;

public sealed record GetThreadsMetricsQuery(
    string MediaId
) : IQuery<ThreadsMetricsDto>;

public sealed record GetThreadsMetricsByDistributionQuery(
    Guid PostId,
    Guid DistributionId
) : IQuery<ThreadsMetricsDto>;

public sealed record GetThreadsAccountMetricsQuery(
    string? Since = null,
    string? Until = null
) : IQuery<ThreadsAccountMetricsDto>;
