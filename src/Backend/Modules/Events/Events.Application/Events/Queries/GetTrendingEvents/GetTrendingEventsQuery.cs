using Events.Application.Events.Queries.GetEvents;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetTrendingEvents;

public record GetTrendingEventsQuery : PagedQuery, IQuery<PagedResult<EventResponse>>
{
    public int Days { get; init; } = 7;
}


