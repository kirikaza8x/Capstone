using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserInterestScore : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string Category { get; private set; } = default!;
        public double InterestScore { get; private set; }
        public DateTime LastInteractionAt { get; private set; }

        private UserInterestScore() { }

        public static UserInterestScore Create(Guid userId, string category, double initialScore)
        {
            return new UserInterestScore
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = category,
                InterestScore = initialScore,
                LastInteractionAt = DateTime.UtcNow
            };
        }

        public void UpdateScore(double delta)
        {
            InterestScore += delta;
            LastInteractionAt = DateTime.UtcNow;
        }

        /// <summary>
        /// ALGORITHM: Exponential Time Decay
        /// Formula: score = score × decayFactor
        /// where decayFactor = exp(-λ × time)
        /// </summary>
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
            // Event sourcing hook
        }
    }
}