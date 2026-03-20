using Shared.Domain.Data.Repositories;
using Ticketing.Domain.Entities;

namespace Ticketing.Domain.Repositories;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<Order?> GetByIdWithOrderTicketAsync(Guid id, CancellationToken cancellationToken = default);

    // Returns the set of (SessionId, SeatId) pairs that are already committed (Valid/Used) in orders.
    Task<IReadOnlySet<(Guid SessionId, Guid SeatId)>> GetCommittedSeatsAsync(
        IReadOnlyCollection<(Guid SessionId, Guid SeatId)> pairs,
        CancellationToken cancellationToken = default);
}