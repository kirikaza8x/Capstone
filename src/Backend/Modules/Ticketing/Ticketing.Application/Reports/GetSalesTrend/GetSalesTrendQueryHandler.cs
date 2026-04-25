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
        var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(
            query.EventId,
            cancellationToken);

        if (ticketTypes is null || !ticketTypes.Any())
        {
            return Result.Success(new SalesTrendResponse(query.EventId, query.Period.ToString(), []));
        }

        var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();

        var orders = await orderRepository.GetPaidOrdersByTicketTypeIdsAsync(ticketTypeIds, cancellationToken);

        if (orders.Count == 0)
        {
            return Result.Success(new SalesTrendResponse(query.EventId, query.Period.ToString(), []));
        }

        var validOrders = orders
            .Select(o => new
            {
                CreatedAt = o.CreatedAt.GetValueOrDefault(),
                Revenue = o.Tickets
                    .Where(t => t.Status != OrderTicketStatus.Cancelled)
                    .Sum(t => t.Price),
                TicketsSold = o.Tickets.Count(t => t.Status != OrderTicketStatus.Cancelled)
            })
            .Where(o => o.TicketsSold > 0)
            .ToList();

        IEnumerable<SalesTrendPoint> trend = query.Period switch
        {
            SalesTrendPeriod.Week => validOrders
                .Select(o => new
                {
                    Bucket = GetStartOfWeek(o.CreatedAt.Date),
                    o.TicketsSold,
                    o.Revenue
                })
                .GroupBy(x => x.Bucket)
                .OrderBy(g => g.Key)
                .Select(g => new SalesTrendPoint(
                    g.Key,
                    g.Sum(x => x.TicketsSold),
                    g.Sum(x => x.Revenue)
                )),

            _ => validOrders
                .Select(o => new
                {
                    Bucket = o.CreatedAt.Date,
                    o.TicketsSold,
                    o.Revenue
                })
                .GroupBy(x => x.Bucket)
                .OrderBy(g => g.Key)
                .Select(g => new SalesTrendPoint(
                    g.Key,
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
