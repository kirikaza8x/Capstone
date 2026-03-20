using Payments.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Payments.Domain.Repositories
{
    public interface IWalletRepository : IRepository<Wallet, Guid>
    {
        // Get wallet by User ID
        Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Get wallet including the transaction history (important for Debit/Credit logic)
        Task<Wallet?> GetByUserIdWithTransactionsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);

        // Check if a wallet is active/exists for a user
        Task<bool> HasActiveWalletAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}