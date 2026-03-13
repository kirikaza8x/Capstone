using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for CategoryEmbedding.
    /// Category embeddings are the foundation for semantic search and user preference modeling.
    /// Supports both exact category lookup and similarity-based discovery.
    /// </summary>
    public interface ICategoryEmbeddingRepository : IRepository<CategoryEmbedding, Guid>
    {
        // ===== SINGLE LOOKUPS =====

        /// <summary>
        /// Returns the embedding for a specific category by name.
        /// </summary>
        Task<CategoryEmbedding?> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns embeddings for multiple categories in a single query.
        /// Used by UserEmbedding.Recalculate() to fetch weighted category vectors.
        /// </summary>
        Task<List<CategoryEmbedding>> GetByCategoriesAsync(
            IEnumerable<string> categories,
            CancellationToken cancellationToken = default);

        // ===== SIMILARITY SEARCH =====

        /// <summary>
        /// Finds the most similar categories based on cosine similarity.
        /// </summary>
        /// <param name="embedding">Query embedding vector (384 dimensions).</param>
        /// <param name="topN">Number of results to return.</param>
        /// <param name="minSimilarity">Minimum similarity threshold (0-1).</param>
        Task<List<CategoryEmbedding>> FindSimilarCategoriesAsync(
            float[] embedding,
            int topN = 10,
            double minSimilarity = 0.0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Recommends categories for a user based on their embedding.
        /// </summary>
        // Task<List<CategoryEmbedding>> RecommendCategoriesForUserAsync(
        //     Guid userId,
        //     int topN = 20,
        //     CancellationToken cancellationToken = default);

        // ===== SEARCH & DISCOVERY =====

        /// <summary>
        /// Returns categories whose descriptions match a keyword search.
        /// </summary>
        Task<List<CategoryEmbedding>> SearchByDescriptionAsync(
            string keyword,
            int take = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all categories (used for building the embedding index).
        /// Consider pagination for large datasets.
        /// </summary>
        Task<List<CategoryEmbedding>> GetAllCategoriesAsync(
            CancellationToken cancellationToken = default);

        // ===== ANALYTICS =====

        /// <summary>
        /// Returns categories with CTR below a threshold (underperforming recommendations).
        /// </summary>
        Task<List<CategoryEmbedding>> GetLowCTRAsync(
            double threshold = 0.1,
            int minRecommendations = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns categories with CTR above a threshold (top performers).
        /// </summary>
        Task<List<CategoryEmbedding>> GetHighCTRAsync(
            double threshold = 0.3,
            int minRecommendations = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns categories that haven't been updated since the given timestamp.
        /// Used to identify candidates for embedding regeneration.
        /// </summary>
        Task<List<CategoryEmbedding>> GetNotUpdatedSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default);

        // ===== MODEL MANAGEMENT =====

        /// <summary>
        /// Returns all categories using a specific embedding model.
        /// Used when migrating to a new model version.
        /// </summary>
        Task<List<CategoryEmbedding>> GetByModelAsync(
            string modelName,
            CancellationToken cancellationToken = default);

        // ===== CLEANUP =====

        /// <summary>
        /// Returns categories with zero interactions that are safe to remove.
        /// </summary>
        Task<List<CategoryEmbedding>> GetUnusedAsync(
            CancellationToken cancellationToken = default);

        Task<List<CategoryEmbedding>> GetPopularAsync(
    int topN,
    CancellationToken cancellationToken = default);
    }
}
