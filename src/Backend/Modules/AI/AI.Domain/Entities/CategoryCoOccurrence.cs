using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Tracks category co-occurrence patterns for collaborative filtering.
    /// "Users who liked X also liked Y"
    /// 
    /// USE CASE:
    /// - Build item-item similarity matrix
    /// - Discover related categories
    /// - Improve recommendation diversity
    /// 
    /// UPDATE STRATEGY:
    /// - Increment count when user interacts with both categories within session window
    /// - Apply decay over time to forget old patterns
    /// </summary>
    public class CategoryCoOccurrence : AggregateRoot<Guid>
    {
        public string Category1 { get; private set; } = default!;
        public string Category2 { get; private set; } = default!;

        /// <summary>
        /// Number of times these categories co-occurred in user sessions
        /// </summary>
        public int CoOccurrenceCount { get; private set; }

        /// <summary>
        /// Lift score: measures correlation strength
        /// Lift = P(A,B) / (P(A) * P(B))
        /// Lift > 1: positive correlation
        /// Lift = 1: independent
        /// Lift < 1: negative correlation
        /// </summary>
        public double LiftScore { get; private set; }

        /// <summary>
        /// Confidence: P(B|A) = P(A,B) / P(A)
        /// Probability of B given A
        /// </summary>
        public double ConfidenceAtoB { get; private set; }

        /// <summary>
        /// Confidence: P(A|B) = P(A,B) / P(B)
        /// </summary>
        public double ConfidenceBtoA { get; private set; }

        /// <summary>
        /// When this co-occurrence was last updated
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Category1 interaction count (for lift calculation)
        /// </summary>
        public int Category1TotalCount { get; private set; }

        /// <summary>
        /// Category2 interaction count (for lift calculation)
        /// </summary>
        public int Category2TotalCount { get; private set; }

        /// <summary>
        /// Total sessions analyzed (for probability calculation)
        /// </summary>
        public int TotalSessions { get; private set; }

        private CategoryCoOccurrence() { }

        // ===== FACTORY METHOD =====
        public static CategoryCoOccurrence Create(
            string category1,
            string category2,
            int totalSessions = 1)
        {
            if (string.IsNullOrWhiteSpace(category1) || string.IsNullOrWhiteSpace(category2))
                throw new ArgumentException("Categories cannot be empty.");

            if (category1 == category2)
                throw new ArgumentException("Categories must be different.", nameof(category2));

            // Enforce consistent ordering to avoid duplicates (A,B) vs (B,A)
            if (string.Compare(category1, category2, StringComparison.Ordinal) > 0)
            {
                (category1, category2) = (category2, category1);
            }

            var now = DateTime.UtcNow;

            return new CategoryCoOccurrence
            {
                Id = Guid.NewGuid(),
                Category1 = category1.ToLowerInvariant().Trim(),
                Category2 = category2.ToLowerInvariant().Trim(),
                CoOccurrenceCount = 1,
                TotalSessions = totalSessions,
                LiftScore = 1.0, // Initial estimate
                ConfidenceAtoB = 1.0,
                ConfidenceBtoA = 1.0,
                LastUpdated = now,
                CreatedAt = now,
                Category1TotalCount = 1,
                Category2TotalCount = 1
            };
        }

        // ===== UPDATE METHODS =====

        /// <summary>
        /// Increments co-occurrence count when both categories appear in a session
        /// </summary>
        public void IncrementCoOccurrence(
            int category1TotalCount,
            int category2TotalCount,
            int totalSessions)
        {
            CoOccurrenceCount++;
            Category1TotalCount = category1TotalCount;
            Category2TotalCount = category2TotalCount;
            TotalSessions = totalSessions;
            LastUpdated = DateTime.UtcNow;

            RecalculateMetrics();
        }

        /// <summary>
        /// Recalculates lift and confidence scores based on current counts
        /// </summary>
        private void RecalculateMetrics()
        {
            if (TotalSessions == 0) return;

            // P(A,B) = co-occurrence count / total sessions
            double pAB = (double)CoOccurrenceCount / TotalSessions;

            // P(A) = category1 count / total sessions
            double pA = (double)Category1TotalCount / TotalSessions;

            // P(B) = category2 count / total sessions
            double pB = (double)Category2TotalCount / TotalSessions;

            // Lift = P(A,B) / (P(A) * P(B))
            if (pA > 0 && pB > 0)
            {
                LiftScore = pAB / (pA * pB);
            }
            else
            {
                LiftScore = 1.0;
            }

            // Confidence A->B = P(B|A) = P(A,B) / P(A)
            if (pA > 0)
            {
                ConfidenceAtoB = pAB / pA;
            }
            else
            {
                ConfidenceAtoB = 0;
            }

            // Confidence B->A = P(A|B) = P(A,B) / P(B)
            if (pB > 0)
            {
                ConfidenceBtoA = pAB / pB;
            }
            else
            {
                ConfidenceBtoA = 0;
            }
        }

        /// <summary>
        /// Applies time decay to co-occurrence count
        /// </summary>
        public void ApplyDecay(double decayFactor)
        {
            if (decayFactor < 0 || decayFactor > 1)
                throw new ArgumentException("Decay factor must be between 0 and 1.", nameof(decayFactor));

            CoOccurrenceCount = (int)(CoOccurrenceCount * decayFactor);
            LastUpdated = DateTime.UtcNow;

            RecalculateMetrics();
        }

        /// <summary>
        /// Checks if this co-occurrence is stale and can be archived
        /// </summary>
        public bool IsStale(int daysThreshold = 90)
        {
            return (DateTime.UtcNow - LastUpdated).TotalDays > daysThreshold
                && CoOccurrenceCount < 2;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Event sourcing hooks
        }
    }
}
