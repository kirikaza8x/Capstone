using Microsoft.EntityFrameworkCore;
using Order.Domain.Orders;
using Shared.Infrastructure.Data;

namespace Order.Infrastructure.Data.Repositories;


internal sealed class OrderRepository : RepositoryBase<Order.Domain.Orders.Order, Guid>, IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Order.Domain.Orders.Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order.Domain.Orders.Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
}
