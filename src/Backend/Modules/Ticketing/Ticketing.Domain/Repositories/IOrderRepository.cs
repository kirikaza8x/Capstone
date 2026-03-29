using Shared.Domain.Data.Repositories;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Ticketing.Domain.Entities;

namespace Ticketing.Domain.Repositories;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<Order?> GetByIdWithOrderTicketAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetPendingExpiredWithTicketsAsync(
        DateTime expiredBeforeUtc,
        int take,
        CancellationToken cancellationToken = default);

    // Returns the set of (SessionId, SeatId) pairs that are already committed (Valid/Used) in orders.
    Task<IReadOnlySet<(Guid SessionId, Guid SeatId)>> GetCommittedSeatsAsync(
        IReadOnlyCollection<(Guid SessionId, Guid SeatId)> pairs,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByOrderTicketIdAsync(
        Guid orderTicketId,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdWithVouchersAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> GetPagedByUserIdAsync(
        Guid userId,
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> GetPagedByEventIdAsync(
        Guid eventId,
        string? status,
        PagedQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetAllByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<Dictionary<(Guid SessionId, Guid TicketTypeId), int>> GetSoldZoneTicketsCountAsync(
        IEnumerable<(Guid SessionId, Guid TicketTypeId)> sessionTicketTypePairs,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetByUserIdAndEventIdAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetByTicketIdsAsync(IEnumerable<Guid> ticketIds, CancellationToken cancellationToken = default);
}
