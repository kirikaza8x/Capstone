using Shared.Domain.DDD;

namespace AI.Domain.ReadModels
{
    /// <summary>
    /// Represents global popularity statistics for a category across all users.
    /// PURPOSE: Powers cold-start recommendations and Bayesian smoothing.
    /// LIFECYCLE: Updated by background job every N hours with decay applied.
    /// </summary>
    public class GlobalCategoryStat : AggregateRoot<Guid>
    {
        public string Category { get; private set; } = default!;

        /// <summary>
        /// Normalized popularity score (0-100 scale by default, but can use other ranges)
        /// Higher = more popular across platform
        /// </summary>
        public double PopularityScore { get; private set; }

        /// <summary>
        /// Total interaction count (used for Bayesian confidence calculation)
        /// Higher count = more reliable statistic
        /// </summary>
        public int TotalInteractions { get; private set; }

        /// <summary>
        /// When this stat was last recalculated (audit trail)
        /// </summary>
        public DateTime LastCalculated { get; private set; }

        /// <summary>
        /// First time this category appeared in the system (analytics)
        /// </summary>
        public DateTime FirstSeen { get; private set; }

        /// <summary>
        /// Weighted score used internally before normalization
        /// (Helps debug why a category has a certain popularity)
        /// </summary>
        public double RawWeightedScore { get; private set; }

        private GlobalCategoryStat() { }

        // ===== FACTORY METHOD =====
        public static GlobalCategoryStat Create(string category, double score, int count, double rawScore = 0)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));

            if (score < 0)
                throw new ArgumentException("Score cannot be negative.", nameof(score));

            if (count < 0)
                throw new ArgumentException("Count cannot be negative.", nameof(count));

            var now = DateTime.UtcNow;

            return new GlobalCategoryStat
            {
                Id = Guid.NewGuid(),
                Category = category.ToLowerInvariant().Trim(),
                PopularityScore = score,
                TotalInteractions = count,
                RawWeightedScore = rawScore > 0 ? rawScore : score,
                LastCalculated = now,
                FirstSeen = now
            };
        }

        // ===== UPDATE METHODS =====

        /// <summary>
        /// Updates statistics with new data from the background job.
        /// This REPLACES the old values (not adds to them).
        /// </summary>
        public void UpdateStats(double newScore, int newTotalCount, double? newRawScore = null)
        {
            if (newScore < 0)
                throw new ArgumentException("Score cannot be negative.", nameof(newScore));

            if (newTotalCount < 0)
                throw new ArgumentException("Count cannot be negative.", nameof(newTotalCount));

            PopularityScore = newScore;
            TotalInteractions = newTotalCount;
            RawWeightedScore = newRawScore ?? newScore;
            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// Applies exponential decay to the popularity score.
        /// CRITICAL: This should be applied to ALL categories in the background job,
        /// not just inactive ones, to ensure fair recency weighting.
        /// </summary>
        /// <param name="decayFactor">Multiplier (0.0 - 1.0). Example: 0.9 = 10% reduction</param>
        public void ApplyDecay(double decayFactor)
        {
            if (decayFactor < 0 || decayFactor > 1)
                throw new ArgumentException("Decay factor must be between 0 and 1.", nameof(decayFactor));

            PopularityScore *= decayFactor;
            RawWeightedScore *= decayFactor;

            // ===== OPTIMIZATION: Floor to zero if too small =====
            // Prevents floating point accumulation and improves query performance
            if (PopularityScore < 0.1)
            {
                PopularityScore = 0;
                RawWeightedScore = 0;
            }

            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds new activity on top of the current (already decayed) score.
        /// This is used when incrementally updating stats rather than recalculating from scratch.
        /// </summary>
        public void AddActivity(double scoreIncrement, int interactionIncrement)
        {
            if (scoreIncrement < 0)
                throw new ArgumentException("Score increment cannot be negative.", nameof(scoreIncrement));

            if (interactionIncrement < 0)
                throw new ArgumentException("Interaction increment cannot be negative.", nameof(interactionIncrement));

            PopularityScore += scoreIncrement;
            RawWeightedScore += scoreIncrement;
            TotalInteractions += interactionIncrement;
            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// Resets the stat to zero (useful for testing or category cleanup)
        /// </summary>
        public void Reset()
        {
            PopularityScore = 0;
            RawWeightedScore = 0;
            TotalInteractions = 0;
            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns true if this category is "dead" and should be archived
        /// </summary>
        public bool IsStale(int daysThreshold = 90)
        {
            return (DateTime.UtcNow - LastCalculated).TotalDays > daysThreshold
                && PopularityScore < 1.0
                && TotalInteractions == 0;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event sourcing logic if needed
            // Example: GlobalCategoryStatUpdated, GlobalCategoryStatDecayed events
        }
    }
}