using AI.Domain.Entities;
using AI.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Data.Repository;

public class OrganizerAiQuotaRepository(AIModuleDbContext dbContext)
    : RepositoryBase<OrganizerAiQuota, Guid>(dbContext), IOrganizerAiQuotaRepository
{
    private readonly DbSet<OrganizerAiQuota> _dbSet = dbContext.Set<OrganizerAiQuota>();

    public async Task<OrganizerAiQuota?> GetByOrganizerIdAsync(Guid organizerId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.OrganizerId == organizerId, ct);
    }
}
