using Shared.Application.Abstractions.EventBus;

namespace Users.IntegrationEvents
{

    /// <summary>
    /// Integration event published when a user’s password is changed.
    /// </summary>
    public sealed record PasswordIntegrationChangedEvent : IntegrationEvent
    {
        public Guid UserId { get; init; }
        public DateTime ChangedAt { get; init; }

        public PasswordIntegrationChangedEvent(Guid userId, DateTime changedAt)
            : base(Guid.NewGuid(), DateTime.UtcNow)
        {
            UserId = userId;
            ChangedAt = changedAt;
        }
    }
}
