using Ticketing.PublicApi.Records;

namespace Ticketing.PublicApi;

public interface ITicketingPublicApi
{
    // Called at payment initiation — gets ticket breakdown for an order
    // Returns null if order not found or does not belong to userId
    Task<OrderDetails?> GetOrderAsync(
        Guid orderId,
        Guid userId,
        bool requirePaid = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-validates the voucher attached to an order before charging the user.
    /// Returns null if no voucher is attached (valid — proceed normally).
    /// Returns a result with IsValid=false if MaxUse is already exhausted.
    /// </summary>
    Task<VoucherValidationResult?> ValidateOrderVoucherAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetZoneLockedCountsAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetSeatLockedCountsByTicketTypeAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetSoldCountsAsync(Guid eventSessionId, IEnumerable<Guid> ticketTypeIds, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetOrdersByEventIdAsync(Guid eventSessionId, CancellationToken cancellationToken);

    Task<TicketingMetricsDto> GetTicketingMetricsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailySalesTrendDto>> GetSalesTrendAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopEventTicketMetricsDto>> GetTopEventsMetricsAsync(
        int top,
        DateTime? startDate = null,
        IReadOnlyList<Guid>? allowedEventIds = null,
        CancellationToken cancellationToken = default);

}

