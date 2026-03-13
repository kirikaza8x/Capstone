// using Shared.Domain.DDD;

using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Global popularity statistics for a category, computed across all users.
    ///
    /// MOVED FROM ReadModels: this aggregate has mutable behaviour (decay, update, reset)
    /// so it belongs in Entities, not ReadModels. A separate flat DTO can serve the read side.
    ///
    /// PURPOSE:
    ///   - Cold-start recommendations for new users
    ///   - Bayesian smoothing (blend personal score with global popularity)
    ///   - Trending category detection
    ///
    /// LIFECYCLE: Updated by a background job on a fixed schedule.
    /// ApplyDecay() is called on every record each run — not just inactive ones.
    /// </summary>
    public class GlobalCategoryStat : AggregateRoot<Guid>
    {
        public string Category { get; private set; } = default!;
        public double PopularityScore { get; private set; }
        public int TotalInteractions { get; private set; }
        public DateTime LastCalculated { get; private set; }
        public DateTime FirstSeen { get; private set; }
        public double RawWeightedScore { get; private set; }

        /// <summary>
        /// Tracks interactions since the last background job run.
        /// Reset to 0 after each job cycle so recency signals stay clean.
        /// </summary>
        public int RecentInteractions { get; private set; }

        private const double ScoreFloor = 0.1;

        private GlobalCategoryStat() { }

        public static GlobalCategoryStat Create(
            string category,
            double score = 0,
            int count = 0,
            double rawScore = 0)
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
                RecentInteractions = 0,
                RawWeightedScore = rawScore > 0 ? rawScore : score,
                LastCalculated = now,
                FirstSeen = now,
                CreatedAt = now
            };
        }

        // ── Update methods ────────────────────────────────────────────────────

        /// <summary>
        /// Replaces computed stats after a full recalculation pass.
        /// Also resets RecentInteractions since the new score already includes them.
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
            RecentInteractions = 0;
            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// Exponential decay on popularity. Apply to ALL categories each job run —
        /// not just inactive ones — to maintain fair recency weighting.
        ///
        /// decayFactor: value in (0, 1]. 0.95 = 5% reduction per run.
        /// </summary>
        public void ApplyDecay(double decayFactor)
        {
            if (decayFactor is <= 0 or > 1)
                throw new ArgumentException(
                    "Decay factor must be in range (0, 1].", nameof(decayFactor));

            PopularityScore *= decayFactor;
            RawWeightedScore *= decayFactor;

            if (PopularityScore < ScoreFloor)
            {
                PopularityScore = 0;
                RawWeightedScore = 0;
            }

            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// Accumulates incremental activity between full recalculation runs.
        /// Also updates RecentInteractions so IsStale() can use it.
        /// </summary>
        public void AddActivity(double scoreIncrement, int interactionIncrement)
        {
            if (scoreIncrement < 0)
                throw new ArgumentException(
                    "Score increment cannot be negative.", nameof(scoreIncrement));
            if (interactionIncrement < 0)
                throw new ArgumentException(
                    "Interaction increment cannot be negative.", nameof(interactionIncrement));

            PopularityScore += scoreIncrement;
            RawWeightedScore += scoreIncrement;
            TotalInteractions += interactionIncrement;
            RecentInteractions += interactionIncrement;
            LastCalculated = DateTime.UtcNow;
        }

        public void Reset()
        {
            PopularityScore = 0;
            RawWeightedScore = 0;
            TotalInteractions = 0;
            RecentInteractions = 0;
            LastCalculated = DateTime.UtcNow;
        }

        /// <summary>
        /// A category is stale when it has had no recent activity AND its score has decayed
        /// below the floor. Uses RecentInteractions (not cumulative TotalInteractions) so
        /// historically popular-but-inactive categories are correctly flagged.
        /// </summary>
        public bool IsStale(int daysThreshold = 90) =>
            (DateTime.UtcNow - LastCalculated).TotalDays > daysThreshold
            && PopularityScore < 1.0
            && RecentInteractions == 0;

        protected override void Apply(IDomainEvent @event)
        {
            // No domain events raised here yet — GlobalCategoryStat is updated
            // by background jobs, not by user-facing commands.
        }
    }
}