using Shared.Domain.Data.Repositories;
using Ticketing.Domain.Entities;

namespace Ticketing.Domain.Repositories;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<Order?> GetByIdWithTicketsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithVouchersAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetPendingExpiredOrdersAsync(DateTime utcNow, int take, CancellationToken cancellationToken = default);
}