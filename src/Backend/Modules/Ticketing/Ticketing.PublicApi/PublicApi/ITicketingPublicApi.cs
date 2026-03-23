using Ticketing.PublicApi.Records;

namespace Ticketing.PublicApi;

public interface ITicketingPublicApi
{
    // Called at payment initiation — gets ticket breakdown for an order
    // Returns null if order not found or does not belong to userId
    Task<OrderDetails?> GetOrderAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetZoneLockedCountsAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetSeatLockedCountsByTicketTypeAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default);
}

