using Shared.Domain.Data;

namespace Order.Domain.Orders;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}