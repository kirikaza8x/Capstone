using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

public interface IOrganizerAiQuotaRepository : IRepository<OrganizerAiQuota, Guid>
{
    Task<OrganizerAiQuota?> GetByOrganizerIdAsync(Guid organizerId, CancellationToken ct = default);
}
