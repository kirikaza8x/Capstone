using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for UserEmbedding.
    /// User embeddings are rebuilt from UserInterestScores and CategoryEmbeddings.
    /// This repository focuses on retrieval for similarity search and stale detection.
    /// </summary>
    public interface IUserEmbeddingRepository : IRepository<UserEmbedding, Guid>
    {
        // ===== SINGLE LOOKUPS =====

        /// <summary>
        /// Returns the embedding for a specific user.
        /// </summary>
        Task<UserEmbedding?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<Guid>> GetUserIdsAsync(
            CancellationToken ct = default);

        // ===== SIMILARITY SEARCH =====

        /// <summary>
        /// Finds the most similar users based on cosine similarity.
        /// </summary>
        /// <param name="embedding">Query embedding vector (384 dimensions).</param>
        /// <param name="topN">Number of results to return.</param>
        /// <param name="minSimilarity">Minimum similarity threshold (0-1).</param>
        Task<List<UserEmbedding>> FindSimilarUsersAsync(
            float[] embedding,
            int topN = 10,
            double minSimilarity = 0.0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds the most similar users to a given user.
        /// </summary>
        Task<List<UserEmbedding>> FindSimilarUsersToUserAsync(
            Guid userId,
            int topN = 10,
            double minSimilarity = 0.0,
            CancellationToken cancellationToken = default);

        // ===== BULK QUERIES =====

        /// <summary>
        /// Returns all stale embeddings that need recalculation.
        /// Used by the background rebuild job.
        /// </summary>
        Task<List<UserEmbedding>> GetStaleEmbeddingsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns embeddings for multiple users in a single query.
        /// Used for batch similarity computations.
        /// </summary>
        Task<List<UserEmbedding>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all embeddings (used for building the full similarity index).
        /// Consider pagination for large datasets.
        /// </summary>
        Task<List<UserEmbedding>> GetAllEmbeddingsAsync(
            CancellationToken cancellationToken = default);

        // ===== ANALYTICS =====

        /// <summary>
        /// Returns the count of embeddings with confidence below a threshold.
        /// </summary>
        Task<int> GetLowConfidenceCountAsync(
            double threshold = 0.5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns embeddings that haven't been calculated since the given timestamp.
        /// </summary>
        Task<List<UserEmbedding>> GetNotCalculatedSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default);

        // ===== CLEANUP =====

        /// <summary>
        /// Returns embeddings that are stale and haven't been updated in a long time.
        /// Safe to archive or delete.
        /// </summary>
        Task<List<UserEmbedding>> GetArchivableAsync(
            int daysThreshold = 180,
            CancellationToken cancellationToken = default);
        Task UpsertAsync(
            UserEmbedding embedding,
            CancellationToken cancellationToken = default);
        Task<List<UserEmbedding>> GetStaleAsync(
            int batchSize,
            CancellationToken ct = default);
    }
}
