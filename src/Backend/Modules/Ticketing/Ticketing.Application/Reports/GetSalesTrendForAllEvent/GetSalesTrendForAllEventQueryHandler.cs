using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Reports.GetSalesTrendForAllEvent;

internal sealed class GetSalesTrendForAllEventQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetSalesTrendForAllEventQuery, SalesTrendForAllEventResponse>
{
    public async Task<Result<SalesTrendForAllEventResponse>> Handle(
        GetSalesTrendForAllEventQuery query,
        CancellationToken cancellationToken)
    {
        var organizerId = currentUserService.UserId;

        var startDate = query.StartDate.Date;
        var endDate = query.EndDate.Date;

        if (endDate < startDate)
        {
            return Result.Failure<SalesTrendForAllEventResponse>(Error.Validation(
                "SalesTrend.InvalidDateRange",
                "EndDate must be greater than or equal to StartDate."));
        }

        var eventIds = await eventTicketingPublicApi.GetEventIdsByUserIdAsync(
            organizerId,
            cancellationToken);

        if (eventIds.Count == 0)
        {
            return Result.Success(new SalesTrendForAllEventResponse(
                organizerId,
                startDate,
                endDate,
                []));
        }

        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            eventIds,
            cancellationToken);

        var eventItems = new List<EventSalesTrendItem>();

        foreach (var eventId in eventIds)
        {
            var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(
                eventId,
                cancellationToken);

            var title = eventSummaryMap.TryGetValue(eventId, out var summary)
                ? summary.EventTitle
                : string.Empty;

            if (ticketTypes is null || !ticketTypes.Any())
            {
                eventItems.Add(new EventSalesTrendItem(
                    eventId,
                    title,
                    BuildTrend(startDate, endDate, new Dictionary<DateTime, DailyAggregate>())));
                continue;
            }

            var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();

            var orders = await orderRepository.GetPaidOrdersByTicketTypeIdsAsync(
                ticketTypeIds,
                cancellationToken);

            var grouped = orders
                .Where(o => o.CreatedAt.HasValue)
                .Select(o => new
                {
                    Date = o.CreatedAt!.Value.Date,
                    GrossRevenue = o.Tickets
                        .Where(t => t.Status != OrderTicketStatus.Cancelled)
                        .Sum(t => t.Price),
                    NetRevenue = o.TotalPrice,
                    TicketsSold = o.Tickets.Count(t => t.Status != OrderTicketStatus.Cancelled)
                })
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .GroupBy(x => x.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new DailyAggregate(
                        g.Sum(x => x.TicketsSold),
                        g.Sum(x => x.NetRevenue),
                        g.Sum(x => x.GrossRevenue)));

            eventItems.Add(new EventSalesTrendItem(
                eventId,
                title,
                BuildTrend(startDate, endDate, grouped)));
        }

        return Result.Success(new SalesTrendForAllEventResponse(
            organizerId,
            startDate,
            endDate,
            eventItems));
    }

    private static IReadOnlyList<EventSalesTrendPoint> BuildTrend(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyDictionary<DateTime, DailyAggregate> grouped)
    {
        var totalDays = (endDate - startDate).Days + 1;

        return Enumerable.Range(0, totalDays)
            .Select(offset =>
            {
                var day = startDate.AddDays(offset);

                if (grouped.TryGetValue(day, out var value))
                {
                    return new EventSalesTrendPoint(
                        day,
                        value.TicketsSold,
                        value.NetRevenue,
                        value.GrossRevenue);
                }

                return new EventSalesTrendPoint(
                    day,
                    0,
                    0m,
                    0m);
            })
            .ToList();
    }

    private sealed record DailyAggregate(
        int TicketsSold,
        decimal NetRevenue,
        decimal GrossRevenue);
}
