using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.Posts.Queries;

/// <summary>
/// Query to fetch Instagram metrics for a specific distributed post
/// </summary>
public sealed record GetInstagramMetricsQuery(
    Guid PostId,
    Guid DistributionId
) : IQuery<InstagramMetricsDto>;