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

    Task<IEnumerable<(PaymentTransaction Transaction, BatchPaymentItem Item)>>
        GetAllCompletedItemsByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);

    Task<(PaymentTransaction? Transaction, BatchPaymentItem? Item)>
        GetCompletedItemByEventIdAsync(
            Guid eventId,
            Guid userId,
            CancellationToken cancellationToken = default);
}