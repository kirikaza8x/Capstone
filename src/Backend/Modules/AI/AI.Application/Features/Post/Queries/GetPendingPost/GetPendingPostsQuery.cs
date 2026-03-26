using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.Posts.Queries;

public record GetPendingPostsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<IReadOnlyList<PostPendingItemDto>>;