using Events.Application.Events.Queries.GetEvents;
using Events.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsByOrganizer;

public record GetEventsByOrganizerQuery : PagedQuery, IQuery<PagedResult<EventResponse>>
{
    public EventStatus? Status { get; init; }
}