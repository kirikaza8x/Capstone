using Shared.Domain.DDD;

namespace AI.Domain.Entities;

public class OrganizerAiQuota : Entity<Guid>
{
    public Guid OrganizerId { get; private set; }
    public int SubscriptionTokens { get; private set; }
    public int TopUpTokens { get; private set; }

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
}
