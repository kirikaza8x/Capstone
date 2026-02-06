using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserInterestScore : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; } 
        public string Category { get; private set; } = string.Empty;
        public double Score { get; private set; }
        public int TotalInteractions { get; private set; }
        public DateTime LastUpdated { get; private set; }
        private UserInterestScore() { }

        public static UserInterestScore Create(Guid userId, string category, double initialScore)
        {
            return new UserInterestScore
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = category.ToLowerInvariant(),
                Score = initialScore,
                LastUpdated = DateTime.UtcNow,
                TotalInteractions = 1
            };
        }

        /// <summary>
        /// Step 1: Apply Time Decay.
        /// If user hasn't acted in 7 days, score drops.
        /// </summary>
        public void ApplyDecay(double halfLifeInDays)
        {
            double daysElapsed = (DateTime.UtcNow - LastUpdated).TotalDays;
            if (daysElapsed <= 0) return;

            // Math: NewScore = OldScore * (0.5 ^ (Days / HalfLife))
            double decayFactor = Math.Pow(0.5, daysElapsed / halfLifeInDays);
            
            Score *= decayFactor;
            // Note: We do NOT update LastUpdated here; we wait for the score update.
        }

        /// <summary>
        /// Step 2: Add new points.
        /// </summary>
        public void AddScore(double points)
        {
            Score += points;
            TotalInteractions++;    
            LastUpdated = DateTime.UtcNow;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event application logic if needed
        }
    }
}