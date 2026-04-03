using AI.Domain.Enums;
using Shared.Domain.DDD;

namespace AI.Domain.Entities;

public class AiTokenTransaction : Entity<Guid>
{
    public Guid QuotaId { get; private set; }
    public Guid? PackageId { get; private set; }
    public AiTokenTransactionType Type { get; private set; }
    public int Amount { get; private set; }
    public int BalanceAfter { get; private set; }
    public Guid? ReferenceId { get; private set; }

    public OrganizerAiQuota Quota { get; private set; } = null!;
    public AiPackage? Package { get; private set; }

    private AiTokenTransaction() { }

    public static AiTokenTransaction Create(
        Guid quotaId,
        Guid? packageId,
        AiTokenTransactionType type,
        int amount,
        int balanceAfter,
        Guid? referenceId)
    {
        return new AiTokenTransaction
        {
            Id = Guid.NewGuid(),
            QuotaId = quotaId,
            PackageId = packageId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
