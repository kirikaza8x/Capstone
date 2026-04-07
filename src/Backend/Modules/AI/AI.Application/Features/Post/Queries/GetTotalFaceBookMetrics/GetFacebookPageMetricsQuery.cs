using Marketing.Application.Posts.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Queries;

public sealed record GetFacebookPageMetricsQuery(
) : IQuery<FacebookPageMetricsDto>;
