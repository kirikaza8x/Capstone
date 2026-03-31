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
        // get all ticket types for the event
        var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(query.EventId, cancellationToken);

        if (ticketTypes is null || !ticketTypes.Any())
        {
            return Result.Success(new SalesTrendResponse(query.EventId, query.Period.ToString(), []));
        }

        var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();

        // Get paid orders
        var orders = await orderRepository.GetPaidOrdersByTicketTypeIdsAsync(ticketTypeIds, cancellationToken);

        if (orders is null || orders.Count == 0)
        {
            return Result.Success(new SalesTrendResponse(query.EventId, query.Period.ToString(), []));
        }

        var validOrders = orders.Select(o => new
        {
            CreatedAt = o.CreatedAt.GetValueOrDefault(),
            Revenue = o.TotalPrice,
            TicketsSold = o.Tickets.Count(t => t.Status != OrderTicketStatus.Cancelled)
        })
        .Where(o => o.TicketsSold > 0)
        .ToList();

        // group by day or week
        IEnumerable<SalesTrendPoint> trend = query.Period switch
        {
            SalesTrendPeriod.Week => validOrders
                .GroupBy(o => GetStartOfWeek(o.CreatedAt.Date))
                .OrderBy(g => g.Key)
                .Select(g => new SalesTrendPoint(
                    $"Tuần {g.Key:dd/MM}",
                    g.Sum(x => x.TicketsSold),
                    g.Sum(x => x.Revenue)
                )),

            _ => validOrders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new SalesTrendPoint(
                    g.Key.ToString("dd/MM"),
                    g.Sum(x => x.TicketsSold),
                    g.Sum(x => x.Revenue)
                ))
        };

        return Result.Success(new SalesTrendResponse(
            query.EventId,
            query.Period.ToString(),
            trend.ToList()));
    }

    private static DateTime GetStartOfWeek(DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}
