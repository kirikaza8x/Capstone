using Shared.Application.Abstractions.Messaging;
using Marketing.Application.Posts.Dtos;
using Shared.Domain.Queries;
using Shared.Domain.Pagination;
using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Queries;

public record GetOrganizerPostsQuery(
    Guid? OrganizerId,

    Guid? EventId = null,

    string? Search = null,

    PostStatus? Status = null,

    DateTime? SubmittedFrom = null,
    DateTime? SubmittedTo = null,

    DateTime? PublishedFrom = null,
    DateTime? PublishedTo = null,

    bool? IsPublished = null,
    bool? HasExternalPostUrl = null

) : PagedQuery, IQuery<PagedResult<PostDto>>;

