using Events.Application.Events.DTOs;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEvents;

public record GetEventsQuery : PagedQuery, IQuery<PagedResult<EventResponse>>
{
    public int? CategoryId { get; init; }
}

public sealed record EventResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? BannerUrl { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime? EventStartAt { get; init; }
    public DateTime? EventEndAt { get; init; }
    public string UrlPath { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<EventCategoryDto> Categories { get; init; } = [];
}
