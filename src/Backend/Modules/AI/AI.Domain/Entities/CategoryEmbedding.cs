using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Represents the vector embedding for a category.
    /// Used for semantic similarity search and content-based recommendations.
    /// 
    /// EMBEDDING MODEL: Uses pre-trained sentence transformers (e.g., all-MiniLM-L6-v2)
    /// - 384 dimensions
    /// - Normalized vectors for cosine similarity
    /// 
    /// STORAGE OPTIONS:
    /// 1. PostgreSQL with pgvector extension (recommended for simplicity)
    /// 2. Qdrant vector database (recommended for scale > 1M vectors)
    /// </summary>
    public class CategoryEmbedding : AggregateRoot<Guid>
    {
        public string Category { get; private set; } = default!;

        /// <summary>
        /// The category description used for generating embeddings
        /// </summary>
        public string Description { get; private set; } = default!;

        /// <summary>
        /// Vector embedding (stored as float array)
        /// Dimension: 384 for all-MiniLM-L6-v2 model
        /// </summary>
        public float[] Embedding { get; private set; } = default!;

        /// <summary>
        /// Dimension of the embedding vector
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Model used to generate this embedding
        /// </summary>
        public string ModelName { get; private set; } = default!;

        /// <summary>
        /// When this embedding was last updated
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Usage count for analytics (how often this category is recommended)
        /// </summary>
        public int RecommendationCount { get; private set; }

        /// <summary>
        /// Click-through count for measuring recommendation quality
        /// </summary>
        public int ClickThroughCount { get; private set; }

        /// <summary>
        /// Calculated CTR (Click-Through Rate)
        /// </summary>
        public double CTR => RecommendationCount > 0 ? (double)ClickThroughCount / RecommendationCount : 0.0;

        private CategoryEmbedding() { }

        // ===== FACTORY METHOD =====
        public static CategoryEmbedding Create(
            string category,
            string description,
            float[] embedding,
            string modelName = "all-MiniLM-L6-v2")
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty.", nameof(category));

            if (embedding == null || embedding.Length == 0)
                throw new ArgumentException("Embedding cannot be null or empty.", nameof(embedding));

            var now = DateTime.UtcNow;

            return new CategoryEmbedding
            {
                Id = Guid.NewGuid(),
                Category = category.ToLowerInvariant().Trim(),
                Description = description?.Trim() ?? string.Empty,
                Embedding = embedding,
                Dimension = embedding.Length,
                ModelName = modelName,
                LastUpdated = now,
                RecommendationCount = 0,
                ClickThroughCount = 0,
                CreatedAt = now
            };
        }

        // ===== UPDATE METHODS =====

        /// <summary>
        /// Updates the embedding vector (e.g., when model is retrained)
        /// </summary>
        public void UpdateEmbedding(float[] newEmbedding, string modelName)
        {
            if (newEmbedding == null || newEmbedding.Length == 0)
                throw new ArgumentException("Embedding cannot be null or empty.", nameof(newEmbedding));

            Embedding = newEmbedding;
            Dimension = newEmbedding.Length;
            ModelName = modelName;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the category description and regenerates embedding
        /// </summary>
        public void UpdateDescription(string newDescription)
        {
            Description = newDescription?.Trim() ?? string.Empty;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Tracks when this category is recommended to a user
        /// </summary>
        public void TrackRecommendation()
        {
            RecommendationCount++;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Tracks when a user clicks on this recommended category
        /// </summary>
        public void TrackClick()
        {
            ClickThroughCount++;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Resets analytics counters (useful for periodic cleanup)
        /// </summary>
        public void ResetAnalytics()
        {
            RecommendationCount = 0;
            ClickThroughCount = 0;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates cosine similarity with another embedding
        /// </summary>
        public double CosineSimilarity(float[] otherEmbedding)
        {
            if (otherEmbedding == null || otherEmbedding.Length != Dimension)
                throw new ArgumentException("Embedding dimension mismatch.", nameof(otherEmbedding));

            double dotProduct = 0;
            double normA = 0;
            double normB = 0;

            for (int i = 0; i < Dimension; i++)
            {
                dotProduct += Embedding[i] * otherEmbedding[i];
                normA += Embedding[i] * Embedding[i];
                normB += otherEmbedding[i] * otherEmbedding[i];
            }

            if (normA == 0 || normB == 0) return 0;

            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Event sourcing hooks for: CategoryEmbeddingUpdated, CategoryEmbeddingCreated
        }
    }
}
