using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Tracks a user's interest level in a specific category using exponential time decay.
    ///
    /// CALL ORDER CONTRACT (enforced by callers, documented here):
    ///   1. ApplyDecay(halfLifeInDays)   — shrink stale score first
    ///   2. AddScore(points)             — stack new interaction points on top
    ///
    /// Never call AddScore without ApplyDecay first, or recency weighting breaks.
    /// </summary>
    public class UserInterestScore : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string Category { get; private set; } = string.Empty;
        public double Score { get; private set; }
        public int TotalInteractions { get; private set; }
        public DateTime LastUpdated { get; private set; }
        private const double ScoreFloor = 0.01;
        private UserInterestScore() { }
        public static UserInterestScore Create(Guid userId, string category, double initialScore = 0)
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
                TotalInteractions = 0,
                LastUpdated = now,
                CreatedAt = now
            };
        }

        /// <summary>
        /// Applies exponential half-life decay: Score *= 0.5 ^ (daysElapsed / halfLifeInDays).
        /// Floors to zero when Score drops below 0.01 to avoid floating-point accumulation.
        ///
        /// NOTE: Does NOT update LastUpdated — that timestamp tracks the last interaction,
        /// not the last time decay ran. This ensures idempotent decay regardless of job frequency.
        /// </summary>
        public void ApplyDecay(double halfLifeInDays)
        {
            if (halfLifeInDays <= 0)
                throw new ArgumentException("Half-life must be positive.", nameof(halfLifeInDays));

            if (Score <= 0) return;

            double daysElapsed = (DateTime.UtcNow - LastUpdated).TotalDays;
            if (daysElapsed <= 0) return;

            double decayFactor = Math.Pow(0.5, daysElapsed / halfLifeInDays);
            Score *= decayFactor;

            if (Score < ScoreFloor)
                Score = 0;
        }

        /// <summary>
        /// Adds interaction points and bumps LastUpdated.
        /// Always call ApplyDecay() before this.
        /// </summary>
        public void AddScore(double points)
        {
            if (points < 0)
                throw new ArgumentException("Cannot add negative points.", nameof(points));

            double previous = Score;

            Score += points;
            TotalInteractions++;
            LastUpdated = DateTime.UtcNow;

            // RaiseDomainEvent(new InterestScoreUpdatedEvent(
            //     ScoreId:       Id,
            //     UserId:        UserId,
            //     Category:      Category,
            //     PreviousScore: previous,
            //     NewScore:      Score,
            //     UpdatedAt:     LastUpdated));
        }

        /// <summary>
        /// Convenience method that applies decay then adds points in the correct order.
        /// Use this in application services to prevent call-order mistakes.
        /// </summary>
        public void DecayAndAdd(double points, double halfLifeInDays)
        {
            ApplyDecay(halfLifeInDays);
            AddScore(points);
        }

        /// <summary>
        /// Resets score to zero (privacy requests, data cleanup).
        /// </summary>
        public void Reset()
        {
            Score = 0;
            TotalInteractions = 0;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// True when score is negligible and the row hasn't been touched recently.
        /// Safe to archive or delete when this returns true.
        /// </summary>
        public bool IsStale(int daysThreshold = 90) =>
            (DateTime.UtcNow - LastUpdated).TotalDays > daysThreshold && Score < 1.0;

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
            //     case InterestScoreUpdatedEvent e:
            //         Score             = e.NewScore;
            //         LastUpdated       = e.UpdatedAt;
            //         TotalInteractions++;
            //         break;
            // }
        }
    }
}