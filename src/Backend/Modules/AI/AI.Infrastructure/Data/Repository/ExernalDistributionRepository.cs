using AI.Infrastructure.Data;
using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Marketing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Marketing.Infrastructure.Persistence.Repositories;

public class ExternalDistributionRepository
    : RepositoryBase<ExternalDistribution, Guid>,
      IExternalDistribuitionRepository
{
    public ExternalDistributionRepository(AIModuleDbContext dbContext)
        : base(dbContext) { }

    public async Task<ExternalDistribution?> GetByPostIdAndPlatformAsync(
        Guid postMarketingId,
        ExternalPlatform platform,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                d => d.PostMarketingId == postMarketingId && d.Platform == platform,
                cancellationToken);
    }
}
