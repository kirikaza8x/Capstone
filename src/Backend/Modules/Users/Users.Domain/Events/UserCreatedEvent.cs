using Shared.Domain.DDD;

namespace Users.Domain.Events
{
    public record UserCreatedEvent(Guid UserId, string Email, string UserName) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType { get; } = nameof(UserCreatedEvent);
    }
}
