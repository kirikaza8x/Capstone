using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Represents a user's preference as a vector in embedding space.
    /// Built by taking a weighted average of the category embeddings the user has interacted with,
    /// weighted by their current (decayed) UserInterestScore for each category.
    ///
    /// NORMALISATION CONTRACT: the stored Embedding is always L2-normalised (unit length).
    /// This enables direct cosine similarity via dot product with no extra division.
    ///
    /// REBUILD TRIGGER: mark stale via a flag when new UserBehaviorLogs arrive;
    /// a background job calls Recalculate() on all stale records.
    /// </summary>
    public class UserEmbedding : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public float[] Embedding { get; private set; } = default!;
        public int Dimension { get; private set; }
        public int InteractionCount { get; private set; }
        public double Confidence { get; private set; }
        public DateTime LastCalculated { get; private set; }
        public bool IsStale { get; private set; }

        private readonly List<string> _contributingCategories = new();
        public IReadOnlyCollection<string> ContributingCategories =>
            _contributingCategories.AsReadOnly();

        private UserEmbedding() { }

        public static UserEmbedding Create(
            Guid userId,
            float[] embedding,
            List<string> contributingCategories)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (embedding is null || embedding.Length == 0)
                throw new ArgumentException("Embedding cannot be null or empty.", nameof(embedding));
            ArgumentNullException.ThrowIfNull(contributingCategories);

            var now = DateTime.UtcNow;

            var entity = new UserEmbedding
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Embedding = L2Normalise(embedding),
                Dimension = embedding.Length,
                InteractionCount = contributingCategories.Count,
                LastCalculated = now,
                CreatedAt = now,
                IsStale = false,
                Confidence = CalculateConfidence(contributingCategories.Count, daysElapsed: 0)
            };

            entity._contributingCategories.AddRange(contributingCategories);
            return entity;
        }

        // ── Mutation ──────────────────────────────────────────────────────────

        /// <summary>
        /// Recomputes the embedding as a weighted centroid of category embeddings.
        /// Each input embedding is L2-normalised before weighting so magnitude differences
        /// don't skew the result.
        ///
        /// FIXES from original:
        ///   - Dimension guard now checks incoming vectors against each other, not against
        ///     the old stored embedding.
        ///   - Each category embedding is normalised before weighting.
        ///   - Confidence is calculated with the real daysElapsed, not a hardcoded 0.
        /// </summary>
        public void Recalculate(
            Dictionary<string, float[]> categoryEmbeddings,
            Dictionary<string, double> weights)
        {
            if (categoryEmbeddings is null || categoryEmbeddings.Count == 0)
                throw new ArgumentException(
                    "Category embeddings cannot be empty.", nameof(categoryEmbeddings));

            int expectedDim = categoryEmbeddings.Values.First().Length;

            if (categoryEmbeddings.Values.Any(e => e.Length != expectedDim))
                throw new ArgumentException(
                    "All category embeddings must have the same dimension.",
                    nameof(categoryEmbeddings));

            var centroid = new float[expectedDim];
            double totalWeight = 0;

            foreach (var (category, rawEmbedding) in categoryEmbeddings)
            {
                float[] normalised = L2Normalise(rawEmbedding);
                double weight = weights.TryGetValue(category, out var w) ? Math.Max(w, 0) : 1.0;

                for (int i = 0; i < expectedDim; i++)
                    centroid[i] += (float)(normalised[i] * weight);

                totalWeight += weight;
            }

            if (totalWeight > 0)
                for (int i = 0; i < expectedDim; i++)
                    centroid[i] /= (float)totalWeight;

            double daysElapsed = (DateTime.UtcNow - LastCalculated).TotalDays;

            Embedding = L2Normalise(centroid);
            Dimension = expectedDim;
            IsStale = false;
            LastCalculated = DateTime.UtcNow;
            InteractionCount = categoryEmbeddings.Count;
            Confidence = CalculateConfidence(categoryEmbeddings.Count, daysElapsed);

            _contributingCategories.Clear();
            _contributingCategories.AddRange(categoryEmbeddings.Keys);

            // RaiseDomainEvent(new UserEmbeddingRecalculatedEvent(
            //     UserId:                    UserId,
            //     Dimension:                 Dimension,
            //     ContributingCategoryCount: InteractionCount,
            //     Confidence:                Confidence,
            //     CalculatedAt:              LastCalculated));
        }

        /// <summary>
        /// Marks this embedding as needing a rebuild.
        /// Called whenever new UserBehaviorLogs arrive for this user.
        /// </summary>
        public void MarkStale() => IsStale = true;

        /// <summary>
        /// Direct vector update — used by incremental online learning pipelines.
        /// Caller is responsible for supplying a unit-normalised vector.
        /// </summary>
        public void UpdateEmbedding(float[] newEmbedding)
        {
            if (newEmbedding is null || newEmbedding.Length == 0)
                throw new ArgumentException(
                    "Embedding cannot be null or empty.", nameof(newEmbedding));

            Embedding = L2Normalise(newEmbedding);
            Dimension = newEmbedding.Length;
            LastCalculated = DateTime.UtcNow;
            IsStale = false;
        }

        /// <summary>
        /// Adds a contributing category to the embedding.
        /// Used when rebuilding embeddings incrementally.
        /// </summary>
        public void AddContributingCategory(string category)
        {
            if (!_contributingCategories.Contains(category))
                _contributingCategories.Add(category);
        }

        // ── Similarity ────────────────────────────────────────────────────────

        /// <summary>
        /// Cosine similarity against another user embedding.
        /// Because both vectors are unit-normalised, this is simply the dot product.
        /// </summary>
        public double CosineSimilarity(UserEmbedding other)
        {
            if (other is null || other.Dimension != Dimension) return 0;
            return DotProduct(Embedding, other.Embedding, Dimension);
        }

        /// <summary>
        /// Cosine similarity against a raw category embedding (not required to be normalised).
        /// </summary>
        public double CosineSimilarity(float[] categoryEmbedding)
        {
            if (categoryEmbedding is null || categoryEmbedding.Length != Dimension) return 0;

            float[] normalised = L2Normalise(categoryEmbedding);
            return DotProduct(Embedding, normalised, Dimension);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static float[] L2Normalise(float[] vector)
        {
            double norm = Math.Sqrt(vector.Sum(x => (double)x * x));
            if (norm == 0) return vector;

            var result = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
                result[i] = (float)(vector[i] / norm);
            return result;
        }

        private static double DotProduct(float[] a, float[] b, int length)
        {
            double sum = 0;
            for (int i = 0; i < length; i++)
                sum += a[i] * b[i];
            return sum;
        }

        /// <summary>
        /// Confidence is a sigmoid over interaction count, time-decayed by a 7-day half-life.
        /// daysElapsed should reflect how stale this embedding was before recalculation.
        /// </summary>
        private static double CalculateConfidence(int interactionCount, double daysElapsed)
        {
            double countFactor = 1.0 / (1.0 + Math.Exp(-0.5 * (interactionCount - 10)));
            double timeFactor = Math.Pow(0.5, daysElapsed / 7.0);
            return Math.Min(1.0, countFactor * timeFactor);
        }

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
            //     case UserEmbeddingRecalculatedEvent e:
            //         Confidence     = e.Confidence;
            //         LastCalculated = e.CalculatedAt;
            //         IsStale        = false;
            //         break;
            // }
        }
    }
}