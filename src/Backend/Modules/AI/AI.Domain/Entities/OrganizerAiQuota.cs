using AI.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;

namespace AI.Domain.Entities;

public class OrganizerAiQuota : Entity<Guid>
{
    public Guid OrganizerId { get; private set; }
    public int SubscriptionTokens { get; private set; }
    public int TopUpTokens { get; private set; }
    public DateTime? SubscriptionExpiresAtUtc { get; private set; }

    private OrganizerAiQuota() { }

    public static OrganizerAiQuota Create(Guid organizerId)
    {
        return new OrganizerAiQuota
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            SubscriptionTokens = 0,
            TopUpTokens = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public int TotalTokens => SubscriptionTokens + TopUpTokens;

    public Result AddSubscriptionTokens(int tokens, DateTime expiresAtUtc)
    {
        if (tokens <= 0)
            return Result.Failure(AiQuotaErrors.TokenAmountMustBePositive);

        if (expiresAtUtc <= DateTime.UtcNow)
            return Result.Failure(AiQuotaErrors.InvalidSubscriptionExpiry);

        SubscriptionTokens += tokens;
        SubscriptionExpiresAtUtc = expiresAtUtc;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddTopUpTokens(int tokens)
    {
        if (tokens <= 0)
            return Result.Failure(AiQuotaErrors.TokenAmountMustBePositive);

        TopUpTokens += tokens;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result<(int SubscriptionUsed, int TopUpUsed)> ConsumeTokens(int tokens)
    {
        if (tokens <= 0)
            return Result.Failure<(int SubscriptionUsed, int TopUpUsed)>(AiQuotaErrors.TokenAmountMustBePositive);

        if (TotalTokens < tokens)
            return Result.Failure<(int SubscriptionUsed, int TopUpUsed)>(
                AiQuotaErrors.InsufficientTokens(tokens, TotalTokens));

        var useSubscription = Math.Min(tokens, SubscriptionTokens);
        var useTopUp = tokens - useSubscription;

        SubscriptionTokens -= useSubscription;
        TopUpTokens -= useTopUp;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success((useSubscription, useTopUp));
    }

    public int ExpireSubscriptionTokens(DateTime utcNow)
    {
        if (!SubscriptionExpiresAtUtc.HasValue || SubscriptionExpiresAtUtc.Value > utcNow || SubscriptionTokens <= 0)
            return 0;

        var expired = SubscriptionTokens;
        SubscriptionTokens = 0;
        SubscriptionExpiresAtUtc = null;
        ModifiedAt = utcNow;

        return expired;
    }
}
