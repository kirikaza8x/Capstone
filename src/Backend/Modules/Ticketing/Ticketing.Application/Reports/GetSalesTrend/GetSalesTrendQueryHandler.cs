using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Reports.GetSalesTrend;

internal sealed class GetSalesTrendQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi)
    : IQueryHandler<GetSalesTrendQuery, SalesTrendResponse>
{
    public async Task<Result<SalesTrendResponse>> Handle(
        GetSalesTrendQuery query,
        CancellationToken cancellationToken)
    {
        var startDate = query.StartDate.Date;
        var endDate = query.EndDate.Date;

        if (endDate < startDate)
        {
            return Result.Failure<SalesTrendResponse>(Error.Validation(
                "SalesTrend.InvalidDateRange",
                "EndDate must be greater than or equal to StartDate."));
        }

        var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(
            query.EventId,
            cancellationToken);

        if (ticketTypes is null || !ticketTypes.Any())
        {
            return Result.Success(CreateResponse(
                query.EventId,
                startDate,
                endDate,
                BuildTrend(startDate, endDate, new Dictionary<DateTime, DailyAggregate>())));
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
                GrossRevenue = o.TotalPrice,
                NetRevenue = o.Tickets
                    .Where(t => t.Status != OrderTicketStatus.Cancelled)
                    .Sum(t => t.Price),
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

        var trend = BuildTrend(startDate, endDate, grouped);

        return Result.Success(CreateResponse(
            query.EventId,
            startDate,
            endDate,
            trend));
    }

    private static SalesTrendResponse CreateResponse(
        Guid eventId,
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<SalesTrendPoint> trend)
    {
        return new SalesTrendResponse(
            eventId,
            startDate,
            endDate,
            trend);
    }

    private static IReadOnlyList<SalesTrendPoint> BuildTrend(
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
                    return new SalesTrendPoint(
                        day,
                        value.TicketsSold,
                        value.NetRevenue,
                        value.GrossRevenue);
                }

                return new SalesTrendPoint(
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
