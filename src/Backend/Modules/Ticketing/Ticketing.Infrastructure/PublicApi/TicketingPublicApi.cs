using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Ticketing.Domain.Enums;
using Ticketing.Infrastructure.Data;
using Ticketing.PublicApi;
using Ticketing.PublicApi.Records;

namespace Ticketing.Infrastructure.PublicApi;

internal sealed class TicketingPublicApi(
    TicketingDbContext dbContext,
    IConnectionMultiplexer redis) : ITicketingPublicApi
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

    public async Task<IReadOnlyDictionary<Guid, int>> GetZoneLockedCountsAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default)
    {
        if (ticketTypeIds.Count == 0)
            return new Dictionary<Guid, int>();

        var db = redis.GetDatabase();
        var idList = ticketTypeIds.ToList();

        var keys = idList
            .Select(id => (RedisKey)$"zone_lock:{eventSessionId}:{id}")
            .ToArray();

        var values = await db.StringGetAsync(keys);

        var result = new Dictionary<Guid, int>();
        for (var i = 0; i < idList.Count; i++)
        {
            result[idList[i]] = values[i].HasValue
                ? (int)values[i]
                : 0;
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetSeatLockedCountsByTicketTypeAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default)
    {
        if (ticketTypeIds.Count == 0)
            return new Dictionary<Guid, int>();

        var lockedCounts = await dbContext.OrderTickets
            .AsNoTracking()
            .Where(ot =>
                ot.EventSessionId == eventSessionId &&
                ticketTypeIds.Contains(ot.TicketTypeId) &&
                ot.SeatId.HasValue &&
                ot.Order.Status == OrderStatus.Pending)
            .GroupBy(ot => ot.TicketTypeId)
            .Select(g => new { TicketTypeId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return lockedCounts.ToDictionary(x => x.TicketTypeId, x => x.Count);
    }
}
