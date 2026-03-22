using Payments.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Payments.Domain.Repositories;

public interface IWalletRepository : IRepository<Wallet, Guid>
{
    Task<Wallet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Wallet?> GetByUserIdWithTransactionsAsync(
        Guid userId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Wallet>> GetByUserIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);
}
