using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

public interface IAiTokenTransactionRepository : IRepository<AiTokenTransaction, Guid>
{
    Task<IReadOnlyList<AiTokenTransaction>> GetPurchasedByQuotaIdAsync(
        Guid quotaId,
        CancellationToken ct = default);
}
