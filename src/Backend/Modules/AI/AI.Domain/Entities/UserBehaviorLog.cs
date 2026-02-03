using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserBehaviorLog : AggregateRoot<Guid>
    {
        public Guid SessionId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid EventId { get; private set; }
        public string ActionType { get; private set; } = default!;
        public string? Metadata { get; private set; }

        private UserBehaviorLog() { }

        public static UserBehaviorLog Create(
            Guid sessionId,
            Guid userId,
            Guid eventId,
            string actionType,
            string? metadata = null)
        {
            return new UserBehaviorLog
            {
                Id = Guid.NewGuid(), 
                SessionId = sessionId,
                UserId = userId,
                EventId = eventId,
                ActionType = actionType,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Domain behaviors
        // public double ToInterestDelta(double weight)
        // {
        //     return weight;
        // }

        public void AttachMetadata(string key, string value)
        {
            Metadata = string.IsNullOrEmpty(Metadata)
                ? $"{key}:{value}"
                : $"{Metadata};{key}:{value}";
        }

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
                
            // }
        }
    }
}
