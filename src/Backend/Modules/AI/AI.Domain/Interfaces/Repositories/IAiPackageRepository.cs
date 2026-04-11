using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

public interface IAiPackageRepository : IRepository<AiPackage, Guid>
{
    Task<IReadOnlyList<AiPackage>> GetListAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
