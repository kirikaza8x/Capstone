using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
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
}
