using Microsoft.EntityFrameworkCore;
using Ticketing.Infrastructure.Data;
using Ticketing.PublicApi;

namespace Ticketing.Infrastructure.PublicApi;

internal sealed class TicketingPublicApi(
    TicketingDbContext dbContext)
    : ITicketingPublicApi
{
    public async Task<OrderDetails?> GetOrderAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Tickets)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.Id == orderId && o.UserId == userId,
                cancellationToken);

        if (order is null) return null;

        // Only return tickets that are not cancelled
        // — cancelled tickets should not be paid for
        var validTickets = order.Tickets
            .Where(t => t.Status != Ticketing.Domain.Enums.OrderTicketStatus.Cancelled)
            .ToList();

        if (validTickets.Count == 0) return null;

        // Price per ticket — derived from TotalPrice / ticket count
        // Each ticket has the same price within an order
        // If your Order has per-ticket pricing later, swap this out
        var pricePerTicket = validTickets.Count > 0
            ? order.TotalPrice / validTickets.Count
            : 0m;

        return new OrderDetails(
            OrderId: order.Id,
            UserId: order.UserId,
            TotalAmount: order.TotalPrice,
            Tickets: validTickets
                .Select(t => new OrderTicketDetail(
                    OrderTicketId: t.Id,
                    EventSessionId: t.EventSessionId,
                    Amount: pricePerTicket))
                .ToList());
    }
}
