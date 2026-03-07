using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEvents;

public record GetEventsQuery : PagedQuery, IQuery<PagedResult<EventResponse>>;

public sealed record EventResponse(
    Guid Id,
    string Title,
    string Status,
    string? BannerUrl,
    string Location,
    DateTime? EventStartAt,
    DateTime? EventEndAt,
    string UrlPath,
    int EventCategoryId,
    DateTime CreatedAt);