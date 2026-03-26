using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.Posts.Queries;

public record GetPostsByOrganizerQuery(
    Guid OrganizerId,
    Guid RequesterId,
    string? StatusFilter = null
) : IQuery<IReadOnlyList<PostDto>>;