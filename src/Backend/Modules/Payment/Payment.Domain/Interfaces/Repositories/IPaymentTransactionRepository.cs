using Payment.Domain.ValueObject;
using Payments.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Payments.Domain.Repositories;

public interface IPaymentTransactionRepository : IRepository<PaymentTransaction, Guid>
{
    Task<PaymentTransaction?> GetByTxnRefWithItemsAsync(
        string txnRef,
        CancellationToken cancellationToken = default);

    Task<PaymentTransaction?> GetByIdWithItemsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PaymentTransaction> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentTransaction>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task<(PaymentTransaction Transaction, BatchPaymentItem Item)?>
        GetCompletedItemBySessionIdAsync(
            Guid eventSessionId,
            Guid userId,
            CancellationToken cancellationToken = default);

    Task<IEnumerable<(PaymentTransaction Transaction, BatchPaymentItem Item)>>
        GetAllCompletedItemsBySessionIdAsync(
            Guid eventSessionId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventRevenue>> GetRevenuePerEventAsync(
        CancellationToken cancellationToken = default);

    Task<EventRevenue?> GetRevenueByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetNetRevenueByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventRevenue>> GetNetRevenuePerEventAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundsByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<EventRefundRate?> GetRefundRateByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<EventTransactionSummary?> GetTransactionSummaryByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<GlobalRevenueSummary> GetGlobalRevenueSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventRevenue>> GetTopEventsByRevenueAsync(int topN, bool byNet = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventRevenue>> GetRevenueByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventRevenue>> GetNetRevenueByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalRefundsByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default);

    Task<OrganizerRevenueSummary> GetRevenueSummaryByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<(PaymentTransaction Transaction, BatchPaymentItem Item)>>
    GetAllCompletedItemsByEventIdAsync(Guid eventId, CancellationToken ct);
}
