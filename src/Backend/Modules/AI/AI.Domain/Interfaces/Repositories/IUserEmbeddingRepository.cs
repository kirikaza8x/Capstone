// using AI.Domain.Entities;
// using Shared.Domain.Data.Repositories;

// namespace AI.Domain.Repositories;

// /// <summary>
// /// Repository for UserEmbedding.
// /// Focus: retrieval for similarity search + stale detection for rebuild jobs.
// /// </summary>
// public interface IUserEmbeddingRepository : IRepository<UserEmbedding, Guid>
// {
//     // ─────────────────────────────────────────────────────────────
//     // Single Lookups
//     // ─────────────────────────────────────────────────────────────
    
//     Task<UserEmbedding?> GetByUserIdAsync(
//         Guid userId,
//         CancellationToken cancellationToken = default);

//     Task<bool> ExistsForUserAsync(
//         Guid userId,
//         CancellationToken cancellationToken = default);

//     // ─────────────────────────────────────────────────────────────
//     // ⭐ Similarity Search (pgvector)
//     // ─────────────────────────────────────────────────────────────
    
//     /// <summary>
//     /// Finds the most similar users based on cosine similarity.
//     /// Returns users ordered by similarity (higher = more similar).
//     /// </summary>
//     Task<List<(Guid UserId, double Similarity, int SharedCategories)>> FindSimilarUsersAsync(
//         float[] queryEmbedding,
//         int topN = 10,
//         double minSimilarity = 0.0,
//         CancellationToken cancellationToken = default);

//     /// <summary>
//     /// Finds the most similar users to a given user.
//     /// Convenience wrapper that fetches the user's embedding first.
//     /// </summary>
//     Task<List<(Guid UserId, double Similarity, int SharedCategories)>> FindSimilarUsersToUserAsync(
//         Guid userId,
//         int topN = 10,
//         double minSimilarity = 0.0,
//         CancellationToken cancellationToken = default);

//     // ─────────────────────────────────────────────────────────────
//     // Bulk Queries (for batch jobs)
//     // ─────────────────────────────────────────────────────────────
    
//     Task<List<UserEmbedding>> GetByIdsAsync(
//         IEnumerable<Guid> ids,
//         CancellationToken cancellationToken = default);

//     Task<List<UserEmbedding>> GetAllActiveAsync(
//         CancellationToken cancellationToken = default);

//     /// <summary>
//     /// Returns all stale embeddings that need recalculation.
//     /// Used by the background rebuild job.
//     /// </summary>
//     Task<List<UserEmbedding>> GetStaleEmbeddingsAsync(
//         CancellationToken cancellationToken = default);

//     /// <summary>
//     /// Returns embeddings that haven't been calculated since the given timestamp.
//     /// </summary>
//     Task<List<UserEmbedding>> GetNotCalculatedSinceAsync(
//         DateTime since,
//         CancellationToken cancellationToken = default);

//     // ─────────────────────────────────────────────────────────────
//     // Upsert (UPSERT pattern for user embeddings)
//     // ─────────────────────────────────────────────────────────────
    
//     /// <summary>
//     /// Thread-safe UPSERT: inserts if not exists, updates if exists.
//     /// Returns the saved entity.
//     /// </summary>
//     Task<UserEmbedding> UpsertAsync(
//         UserEmbedding embedding,
//         CancellationToken cancellationToken = default);

//     // ─────────────────────────────────────────────────────────────
//     // Analytics
//     // ─────────────────────────────────────────────────────────────
    
//     Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    
//     Task<int> GetLowConfidenceCountAsync(
//         double threshold = 0.5,
//         CancellationToken cancellationToken = default);

//     // ─────────────────────────────────────────────────────────────
//     // Cleanup
//     // ─────────────────────────────────────────────────────────────
    
//     /// <summary>
//     /// Returns embeddings that are stale and haven't been updated in a long time.
//     /// Safe to archive or delete.
//     /// </summary>
//     Task<List<UserEmbedding>> GetArchivableAsync(
//         int daysThreshold = 180,
//         CancellationToken cancellationToken = default);
// }