
using Payments.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Payments.Domain.Repositories
{
    public interface IPaymentTransactionRepository : IRepository<PaymentTransaction, Guid>
    {
        // Find a transaction by the internal reference sent to VnPay (vnp_TxnRef)
        Task<PaymentTransaction?> GetByTxnRefAsync(string txnRef, CancellationToken cancellationToken = default);

        // Find by the actual Gateway Transaction ID (vnp_TransactionNo)
        Task<PaymentTransaction?> GetByGatewayTransactionNoAsync(string transactionNo, CancellationToken cancellationToken = default);

        // Get all transactions for a specific user
        Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Get pending transactions that might need a status check (Requery)
        Task<IEnumerable<PaymentTransaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);

        Task<PaymentTransaction?> GetCompletedByEventIdAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<PaymentTransaction>> GetAllCompletedByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    }
}
