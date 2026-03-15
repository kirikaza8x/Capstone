using Events.Application.Events.DTOs;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsForStaff;

public record GetEventsForStaffQuery : PagedQuery, IQuery<PagedResult<EventsForStaffResponse>>
{
    public string? Title { get; init; }
    
    public string? Statuses { get; init; } 
}

public sealed record EventsForStaffResponse
{
    public Guid Id { get; init; }
    public Guid OrganizerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? BannerUrl { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime? EventStartAt { get; init; }
    public DateTime? EventEndAt { get; init; }
    public DateTime CreatedAt { get; init; }


    public IReadOnlyList<EventCategoryDto> Categories { get; init; } = [];
}