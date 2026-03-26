using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.Posts.Queries;

public record GetPostByIdQuery(
    Guid PostId,
    Guid RequesterId,
    bool IsAdmin = false
) : IQuery<PostDetailDto>;