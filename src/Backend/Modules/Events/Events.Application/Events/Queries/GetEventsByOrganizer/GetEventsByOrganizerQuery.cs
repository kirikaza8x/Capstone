using Events.Application.Events.Queries.GetEvents;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsByOrganizer;

public record GetEventsByOrganizerQuery : PagedQuery, IQuery<PagedResult<EventResponse>>
{
    public string? Statuses { get; init; }
    public string? Title { get; init; }
}