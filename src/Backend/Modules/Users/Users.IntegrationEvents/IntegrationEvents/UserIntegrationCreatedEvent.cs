using Shared.Application.Abstractions.EventBus;

namespace Users.IntegrationEvents
{
    /// <summary>
    /// Integration event published when a new user is created.
    /// </summary>
    public sealed record UserIntegrationCreatedEvent : IntegrationEvent
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = default!;
        public string UserName { get; init; } = default!;
        public List<string> Roles { get; init; } = new();
        public DateTime CreatedAt { get; init; }

        public UserIntegrationCreatedEvent(Guid userId, string email, string userName, List<string> roles, DateTime createdAt)
            : base(Guid.NewGuid(), DateTime.UtcNow)
        {
            UserId = userId;
            Email = email;
            UserName = userName;
            Roles = roles;
            CreatedAt = createdAt;
        }
    }

}
