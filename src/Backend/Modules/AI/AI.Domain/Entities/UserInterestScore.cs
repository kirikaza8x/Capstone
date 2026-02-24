using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Represents a user's interest level in a specific category.
    /// THREAD-SAFE: Designed to be updated via repository UPSERT pattern.
    /// DECAY MODEL: Exponential time-based decay to prioritize recent interactions.
    /// </summary>
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
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));
            
            if (initialScore < 0)
                throw new ArgumentException("Initial score cannot be negative.", nameof(initialScore));

            var now = DateTime.UtcNow;

            return new UserInterestScore
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = category.ToLowerInvariant().Trim(),
                Score = initialScore,
                TotalInteractions = 1,
                LastUpdated = now,
                CreatedAt = now
            };
        }

        /// <summary>
        /// Applies exponential time decay to the score.
        /// Formula: NewScore = OldScore * (0.5 ^ (DaysElapsed / HalfLifeInDays))
        /// 
        /// IMPORTANT: This should be called BEFORE adding new points to ensure
        /// the score reflects the true recency-weighted value.
        /// </summary>
        /// <param name="halfLifeInDays">Number of days for score to decay to 50%</param>
        public void ApplyDecay(double halfLifeInDays)
        {
            if (halfLifeInDays <= 0)
                throw new ArgumentException("Half-life must be positive.", nameof(halfLifeInDays));

            double daysElapsed = (DateTime.UtcNow - LastUpdated).TotalDays;
            
            // No time has passed - no decay needed
            if (daysElapsed <= 0) return;

            // Exponential decay formula
            double decayFactor = Math.Pow(0.5, daysElapsed / halfLifeInDays);
            
            Score *= decayFactor;

            // ===== OPTIMIZATION: Floor very small scores to zero =====
            // Prevents floating point accumulation and improves query performance
            if (Score < 0.01) Score = 0;

            // NOTE: We do NOT update LastUpdated here - that happens in AddScore()
        }

        /// <summary>
        /// Adds new interaction points to the score.
        /// This should be called AFTER ApplyDecay() in the orchestrator.
        /// </summary>
        /// <param name="points">Points to add (must be non-negative)</param>
        public void AddScore(double points)
        {
            if (points < 0)
                throw new ArgumentException("Cannot add negative points.", nameof(points));

            Score += points;
            TotalInteractions++;    
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Resets the score to zero (useful for data cleanup or user privacy requests)
        /// </summary>
        public void Reset()
        {
            Score = 0;
            TotalInteractions = 0;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns true if this score is "stale" and can be archived/deleted
        /// </summary>
        public bool IsStale(int daysThreshold = 90)
        {
            return (DateTime.UtcNow - LastUpdated).TotalDays > daysThreshold && Score < 1.0;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event sourcing logic if needed
            // Example: UserInterestScoreUpdated, UserInterestScoreDecayed events
        }
    }
}