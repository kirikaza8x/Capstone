using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Shared.Domain.Data.Repositories;

namespace Marketing.Domain.Repositories;

public interface IExternalDistribuitionRepository
    : IRepository<ExternalDistribution, Guid>
{
    Task<ExternalDistribution?> GetByPostIdAndPlatformAsync(
        Guid postMarketingId,
        ExternalPlatform platform,
        CancellationToken cancellationToken = default);
}
