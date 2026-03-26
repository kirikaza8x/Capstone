using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.Posts.Queries;

public record GetPostsByEventQuery(
    Guid EventId,
    Guid RequesterId,
    bool IsOrganizer = false,
    bool IncludeDrafts = false
) : IQuery<IReadOnlyList<PostDto>>;