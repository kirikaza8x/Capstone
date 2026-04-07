using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Queries;

public sealed record GetFacebookPageMetricsQuery(
    FacebookPeriod Period = FacebookPeriod.Day 
) : IQuery<FacebookPageMetricsDto>;
