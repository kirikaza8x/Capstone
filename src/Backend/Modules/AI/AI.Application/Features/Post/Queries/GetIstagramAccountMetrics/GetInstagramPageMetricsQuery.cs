using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Queries;

/// <summary>
/// Query to fetch aggregate Instagram account metrics for a specified period
/// </summary>
public sealed record GetInstagramPageMetricsQuery(
    InstagramPeriod Period = InstagramPeriod.days_28
) : IQuery<InstagramPageMetricsDto>;