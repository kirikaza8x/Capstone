using Marketing.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Marketing.Domain.Repositories;

public interface IExternalDistribuitionRepository : IRepository<ExternalDistribution, Guid>
{
}