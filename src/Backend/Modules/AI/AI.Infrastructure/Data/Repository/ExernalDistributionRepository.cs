using AI.Infrastructure.Data;
using Marketing.Domain.Entities;
using Marketing.Domain.Repositories;
using Shared.Infrastructure.Data;

namespace Marketing.Infrastructure.Persistence.Repositories;

public class ExternalDistributionRepository
    : RepositoryBase<ExternalDistribution, Guid>,
      IExternalDistribuitionRepository
{
    public ExternalDistributionRepository(AIModuleDbContext dbContext)
        : base(dbContext)
    {
    }
}