using AI.Domain.Entities;
using AI.Domain.Enums;
using AI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Data.Repository;

public class AiTokenTransactionRepository(AIModuleDbContext dbContext)
    : RepositoryBase<AiTokenTransaction, Guid>(dbContext), IAiTokenTransactionRepository
{
    private readonly DbSet<AiTokenTransaction> _dbSet = dbContext.Set<AiTokenTransaction>();

    public async Task<IReadOnlyList<AiTokenTransaction>> GetPurchasedByQuotaIdAsync(
        Guid quotaId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(x => x.Package)
            .Where(x =>
                x.QuotaId == quotaId &&
                x.PackageId.HasValue &&
                x.Package != null &&
                x.Amount > 0 &&
                (x.Type == AiTokenTransactionType.TopUp || x.Type == AiTokenTransactionType.MonthlyGrant))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByReferenceIdAsync(Guid referenceId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(x => x.ReferenceId == referenceId, ct);
    }
}
