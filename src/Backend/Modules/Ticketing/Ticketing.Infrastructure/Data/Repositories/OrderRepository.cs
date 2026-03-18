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

    public async Task<Order?> GetByIdWithTicketsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetByIdWithVouchersAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(x => x.OrderVouchers)
                .ThenInclude(x => x.Voucher)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPendingExpiredOrdersAsync(
        DateTime utcNow,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(x => x.Tickets)
            .Where(x =>
                x.Status == OrderStatus.Pending &&
                x.CreatedAt.HasValue &&
                x.CreatedAt <= utcNow)
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}