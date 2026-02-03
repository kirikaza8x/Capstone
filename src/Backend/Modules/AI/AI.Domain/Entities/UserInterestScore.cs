using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserInterestScore : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public int CategoryId { get; private set; }
        public double InterestScore { get; private set; }
        public DateTime LastInteractionAt { get; private set; }

        // EF Core constructor
        private UserInterestScore() { }

        public static UserInterestScore Create(Guid userId, int categoryId, double initialScore)
        {
            return new UserInterestScore
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = categoryId,
                InterestScore = initialScore,
                LastInteractionAt = DateTime.UtcNow
            };
        }

        // Domain behaviors
        public void UpdateScore(double delta)
        {
            InterestScore += delta;
            LastInteractionAt = DateTime.UtcNow;
        }

        public void ApplyDecay(double decayFactor)
        {
            InterestScore *= decayFactor;
            LastInteractionAt = DateTime.UtcNow;
        }

        public void Reset()
        {
            InterestScore = 0;
            LastInteractionAt = DateTime.UtcNow;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
                
            // }
        }
    }
}
