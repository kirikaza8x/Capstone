using AutoMapper;
using Events.Application.Events.Queries.GetEvents;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Ticketing.PublicApi;

namespace Events.Application.Events.Queries.GetTrendingEvents;

internal sealed class GetTrendingEventsQueryHandler(
    IEventRepository eventRepository,
    ITicketingPublicApi ticketingPublicApi,
    IMapper mapper) : IQueryHandler<GetTrendingEventsQuery, PagedResult<EventResponse>>
{
    // Cap how many top events we pull from the ticketing service to keep memory bounded.
    private const int MaxTopEvents = 200;
    
    public async Task<Result<PagedResult<EventResponse>>> Handle(
        GetTrendingEventsQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        // Fetch top-N trending event IDs ranked by tickets sold in the given window.
        var topMetrics = await ticketingPublicApi.GetTopEventsMetricsAsync(
            top: MaxTopEvents,
            startDate: startDate,
            cancellationToken: cancellationToken);

        if (topMetrics.Count == 0)
            return Result.Success(PagedResult<EventResponse>.Empty);

        var totalCount = topMetrics.Count;
        var pageNumber = request.PageNumber ?? 1;
        var pageSize   = request.PageSize ?? 10;

        // Apply pagination on the ranked list (already ordered by ticket sales desc).
        var pagedEventIds = topMetrics
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => m.EventId)
            .ToList();

        if (pagedEventIds.Count == 0)
            return Result.Success(PagedResult<EventResponse>.Empty);

        // Fetch the actual event data for this page.
        var events = await eventRepository.GetMiniByIdsAsync(pagedEventIds, cancellationToken);

        // Preserve the ticket-count ranking order.
        var orderedEvents = pagedEventIds
            .Select(id => events.FirstOrDefault(e => e.Id == id))
            .Where(e => e is not null)
            .ToList();

        var responseItems = mapper.Map<IReadOnlyList<EventResponse>>(orderedEvents);

        var result = PagedResult<EventResponse>.Create(
            responseItems,
            pageNumber,
            pageSize,
            totalCount);

        return Result.Success(result);
    }
}
