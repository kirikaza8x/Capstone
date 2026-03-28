using Microsoft.EntityFrameworkCore;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Extensions;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Infrastructure.Data.Repositories;

internal sealed class OrderRepository(TicketingDbContext context)
    : RepositoryBase<Order, Guid>(context), IOrderRepository
{
    private readonly TicketingDbContext _context = context;

    public async Task<Order?> GetByIdWithOrderTicketAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Orders
            .Include(o => o.Tickets)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetPendingExpiredWithTicketsAsync(
        DateTime expiredBeforeUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Tickets)
            .Include(o => o.OrderVouchers)
            .Where(o =>
                o.Status == OrderStatus.Pending &&
                o.CreatedAt.HasValue &&
                o.CreatedAt <= expiredBeforeUtc)
            .OrderBy(o => o.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<(Guid SessionId, Guid SeatId)>> GetCommittedSeatsAsync(
        IReadOnlyCollection<(Guid SessionId, Guid SeatId)> pairs,
        CancellationToken cancellationToken = default)
    {
        if (pairs.Count == 0)
            return new HashSet<(Guid, Guid)>();

        var sessionIds = pairs.Select(p => p.SessionId).ToList();
        var seatIds = pairs.Select(p => p.SeatId).ToList();

        var committed = await _context.OrderTickets
            .AsNoTracking()
            .Where(ot =>
                ot.SeatId.HasValue &&
                sessionIds.Contains(ot.EventSessionId) &&
                seatIds.Contains(ot.SeatId!.Value) &&
                (ot.Status == OrderTicketStatus.Valid || ot.Status == OrderTicketStatus.Used) &&
                ot.Order.Status == OrderStatus.Paid)
            .Select(ot => new { ot.EventSessionId, SeatId = ot.SeatId!.Value })
            .ToListAsync(cancellationToken);

        return committed
            .Select(x => (x.EventSessionId, x.SeatId))
            .ToHashSet();
    }

    public async Task<Order?> GetByOrderTicketIdAsync(
        Guid orderTicketId,
        CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Include(o => o.Tickets)
            .FirstOrDefaultAsync(
                o => o.Tickets.Any(t => t.Id == orderTicketId),
                cancellationToken);

    public async Task<Order?> GetByIdWithVouchersAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Include(o => o.OrderVouchers)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<PagedResult<Order>> GetPagedByUserIdAsync(
        Guid userId,
        PagedQuery query,
        CancellationToken cancellationToken = default) =>
        await _context.Orders
            .AsNoTracking()
            .Include(o => o.Tickets)
            .Include(o => o.OrderVouchers)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToPagedResultAsync(query, cancellationToken);

    public async Task<PagedResult<Order>> GetPagedByEventIdAsync(
        Guid eventId,
        string? status,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var ordersQuery = _context.Orders
            .AsNoTracking()
            .Include(o => o.Tickets)
            .Include(o => o.OrderVouchers)
            .Where(o => o.EventId == eventId);

        // Filter by status if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == orderStatus);
            }
        }

        // Sort, page, and return
        return await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderVouchers)
            .Where(o => o.EventId == eventId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<(Guid SessionId, Guid TicketTypeId), int>> GetSoldZoneTicketsCountAsync(
        IEnumerable<(Guid SessionId, Guid TicketTypeId)> sessionTicketTypePairs,
        CancellationToken cancellationToken = default)
    {
        var pairsList = sessionTicketTypePairs.ToList();
        if (pairsList.Count == 0)
        {
            return [];
        }

        // Get list ID for filtering
        var sessionIds = pairsList.Select(p => p.SessionId).Distinct().ToList();
        var ticketTypeIds = pairsList.Select(p => p.TicketTypeId).Distinct().ToList();

        // Query
        var counts = await context.Set<OrderTicket>()
            .Where(t => sessionIds.Contains(t.EventSessionId)
                     && ticketTypeIds.Contains(t.TicketTypeId)
                     && (t.Status == OrderTicketStatus.Valid || t.Status == OrderTicketStatus.Used))
            .GroupBy(t => new { t.EventSessionId, t.TicketTypeId })
            .Select(g => new
            {
                g.Key.EventSessionId,
                g.Key.TicketTypeId,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        // Convert to dictionary
        var result = new Dictionary<(Guid SessionId, Guid TicketTypeId), int>();

        foreach (var item in counts)
        {
            if (pairsList.Contains((item.EventSessionId, item.TicketTypeId)))
            {
                result[(item.EventSessionId, item.TicketTypeId)] = item.Count;
            }
        }

        return result;
    }
}
