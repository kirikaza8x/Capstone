using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

namespace Payments.Infrastructure.Persistence.Repositories;

public class WalletRepository
    : RepositoryBase<Wallet, Guid>, IWalletRepository
{
    public WalletRepository(PaymentModuleDbContext context)
        : base(context) { }

    public async Task<Wallet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<Wallet?> GetByUserIdWithTransactionsAsync(
        Guid userId,
        int limit = 10,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(x => x.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(limit))
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<IEnumerable<Wallet>> GetByUserIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

    public async Task<Wallet?> GetByUserIdIncludeTransacAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
}
