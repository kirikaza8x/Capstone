// Marketing.Application/Posts/Queries/GetFacebookMetricsQuery.cs

using Marketing.Application.Posts.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Queries;

public sealed record GetFacebookMetricsQuery(
    Guid PostId,
    Guid DistributionId
) : IQuery<FacebookMetricsDto>;