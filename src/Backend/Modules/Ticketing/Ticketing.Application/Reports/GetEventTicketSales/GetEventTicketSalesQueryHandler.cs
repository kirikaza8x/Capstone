using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Reports.GetEventTicketSales;

internal sealed class GetEventTicketSalesQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi)
    : IQueryHandler<GetEventTicketSalesQuery, TicketSalesResponse>
{
    public async Task<Result<TicketSalesResponse>> Handle(
        GetEventTicketSalesQuery query,
        CancellationToken cancellationToken)
    {
        // get all ticket types for the event
        var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(query.EventId, cancellationToken);

        if (ticketTypes is null || !ticketTypes.Any())
        {
            return Result.Success(new TicketSalesResponse(
                query.EventId,
                new TicketSalesSummaryDto(0, 0, 0, 0, 0, 0, 0),
                []));
        }

        var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();

        // get orders
        var orders = await orderRepository.GetPaidOrdersByTicketTypeIdsAsync(ticketTypeIds, cancellationToken);

        var allValidTickets = orders != null && orders.Count > 0
            ? orders.SelectMany(o => o.Tickets).Where(t => t.Status != OrderTicketStatus.Cancelled).ToList()
            : [];

        // calculate summary
        int totalOrders = orders?.Count ?? 0;
        int totalTicketsSold = allValidTickets.Count;
        int totalTicketsCheckedIn = allValidTickets.Count(t => t.Status == OrderTicketStatus.Used);

        double checkInRate = totalTicketsSold > 0
            ? Math.Round((double)totalTicketsCheckedIn / totalTicketsSold * 100, 2)
            : 0;

        decimal netRevenue = orders?.Sum(o => o.TotalPrice) ?? 0;
        decimal totalDiscount = orders?.SelectMany(o => o.OrderVouchers).Sum(v => v.DiscountAmount) ?? 0;
        decimal grossRevenue = netRevenue + totalDiscount;

        var summary = new TicketSalesSummaryDto(
            totalOrders, totalTicketsSold, totalTicketsCheckedIn, checkInRate, grossRevenue, totalDiscount, netRevenue);

        // Group sold tickets by ticket type for breakdown
        var soldTicketsGroup = allValidTickets
            .GroupBy(t => t.TicketTypeId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    QuantitySold = g.Count(),
                    QuantityCheckedIn = g.Count(t => t.Status == OrderTicketStatus.Used),
                    Revenue = g.Sum(t => t.Price)
                });


        var breakdown = ticketTypes.Select(tt =>
        {
            var hasSales = soldTicketsGroup.TryGetValue(tt.Id, out var salesData);

            return new TicketTypeSalesDto(
                tt.Id,
                tt.Name,
                tt.Quantity,
                hasSales ? salesData.QuantitySold : 0,
                hasSales ? salesData.QuantityCheckedIn : 0,
                hasSales ? salesData.Revenue : 0m
            );
        }).ToList();

        return Result.Success(new TicketSalesResponse(query.EventId, summary, breakdown));
    }
}
