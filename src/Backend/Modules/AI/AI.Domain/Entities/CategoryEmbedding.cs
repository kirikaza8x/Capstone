using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Vector embedding for a category, used for semantic similarity search.
    ///
    /// EMBEDDING MODEL: sentence-transformers (e.g. all-MiniLM-L6-v2), 384 dimensions.
    /// Stored vectors are L2-normalised for cosine similarity via dot product.
    ///
    /// STORAGE:
    ///   - PostgreSQL + pgvector  (recommended up to ~1M categories)
    ///   - Qdrant                 (recommended above ~1M or when filtering is complex)
    ///
    /// CTR INVARIANT: ClickThroughCount can never exceed RecommendationCount.
    /// Always call TrackRecommendation() before TrackClick().
    /// </summary>
    public class CategoryEmbedding : AggregateRoot<Guid>
    {
        public string Category { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public float[] Embedding { get; private set; } = default!;
        public int Dimension { get; private set; }
        public string ModelName { get; private set; } = default!;
        public DateTime LastUpdated { get; private set; }
        public int RecommendationCount { get; private set; }
        public int ClickThroughCount { get; private set; }

        /// <summary>
        /// Click-through rate. Always in range [0, 1] due to increment ordering invariant.
        /// </summary>
        public double CTR => RecommendationCount > 0
            ? (double)ClickThroughCount / RecommendationCount
            : 0.0;

        private CategoryEmbedding() { }

        public static CategoryEmbedding Create(
            string category,
            string description,
            float[] embedding,
            string modelName = "all-MiniLM-L6-v2")
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));
            if (embedding is null || embedding.Length == 0)
                throw new ArgumentException("Embedding cannot be null or empty.", nameof(embedding));
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("ModelName cannot be empty.", nameof(modelName));

            var now = DateTime.UtcNow;

            return new CategoryEmbedding
            {
                Id = Guid.NewGuid(),
                Category = category.ToLowerInvariant().Trim(),
                Description = description?.Trim() ?? string.Empty,
                Embedding = embedding,
                Dimension = embedding.Length,
                ModelName = modelName.Trim(),
                LastUpdated = now,
                RecommendationCount = 0,
                ClickThroughCount = 0,
                CreatedAt = now
            };
        }

        // ── Update methods ────────────────────────────────────────────────────

        /// <summary>
        /// Replaces the embedding vector (e.g. after model retraining).
        /// </summary>
        public void UpdateEmbedding(float[] newEmbedding, string modelName)
        {
            if (newEmbedding is null || newEmbedding.Length == 0)
                throw new ArgumentException(
                    "Embedding cannot be null or empty.", nameof(newEmbedding));
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("ModelName cannot be empty.", nameof(modelName));

            Embedding = newEmbedding;
            Dimension = newEmbedding.Length;
            ModelName = modelName.Trim();
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the description text. Does NOT regenerate the embedding vector — this
        /// raises a CategoryDescriptionChangedEvent so a handler can schedule regeneration.
        /// </summary>
        public void UpdateDescription(string newDescription)
        {
            var trimmed = newDescription?.Trim() ?? string.Empty;
            if (trimmed == Description) return;

            Description = trimmed;
            LastUpdated = DateTime.UtcNow;

            // RaiseDomainEvent(new CategoryDescriptionChangedEvent(
            //     CategoryEmbeddingId: Id,
            //     Category:            Category,
            //     NewDescription:      Description,
            //     ChangedAt:           LastUpdated));
        }

        // ── Analytics ─────────────────────────────────────────────────────────

        /// <summary>
        /// Call this each time this category is surfaced in a recommendation result.
        /// Must be called before TrackClick() to preserve the CTR invariant.
        /// </summary>
        public void TrackRecommendation()
        {
            RecommendationCount++;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Call this when a user clicks on this recommended category.
        /// Requires TrackRecommendation() to have been called first.
        /// </summary>
        public void TrackClick()
        {
            if (ClickThroughCount >= RecommendationCount)
                throw new InvalidOperationException(
                    $"Cannot track a click for category '{Category}' before tracking a recommendation. " +
                    $"Call TrackRecommendation() first.");

            ClickThroughCount++;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Resets analytics counters for a new measurement period.
        /// </summary>
        public void ResetAnalytics()
        {
            RecommendationCount = 0;
            ClickThroughCount = 0;
            LastUpdated = DateTime.UtcNow;
        }

        // ── Similarity ────────────────────────────────────────────────────────

        /// <summary>
        /// Cosine similarity against an arbitrary embedding vector.
        /// </summary>
        public double CosineSimilarity(float[] otherEmbedding)
        {
            if (otherEmbedding is null || otherEmbedding.Length != Dimension)
                throw new ArgumentException(
                    $"Embedding dimension mismatch. Expected {Dimension}, " +
                    $"got {otherEmbedding?.Length ?? 0}.", nameof(otherEmbedding));

            double dot = 0, normA = 0, normB = 0;

            for (int i = 0; i < Dimension; i++)
            {
                dot += Embedding[i] * otherEmbedding[i];
                normA += Embedding[i] * Embedding[i];
                normB += otherEmbedding[i] * otherEmbedding[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
            //     case CategoryDescriptionChangedEvent e:
            //         Description = e.NewDescription;
            //         LastUpdated = e.ChangedAt;
            //         break;
            // }
        }
    }
}