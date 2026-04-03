using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Data.Repository;

public class AiPackageRepository(AIModuleDbContext dbContext)
    : RepositoryBase<AiPackage, Guid>(dbContext), IAiPackageRepository
{
    private readonly DbSet<AiPackage> _dbSet = dbContext.Set<AiPackage>();

    public async Task<IReadOnlyList<AiPackage>> GetListAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = name.Trim().ToLower();

        var query = _dbSet.AsNoTracking()
            .Where(x => x.Name.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
