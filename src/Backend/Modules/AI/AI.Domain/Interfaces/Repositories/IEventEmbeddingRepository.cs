// using AI.Domain.Entities;
// using Shared.Domain.Data.Repositories;

// namespace AI.Domain.Repositories;

// public interface IEventEmbeddingRepository : IRepository<EventEmbedding, Guid>
// {
//     // ─────────────────────────────────────────────────────────────
//     // Basic Lookups
//     // ─────────────────────────────────────────────────────────────
    
//     Task<EventEmbedding?> GetByEventIdAsync(
//         Guid eventId, 
//         CancellationToken ct = default);

//     Task<bool> ExistsForEventAndModelAsync(
//         Guid eventId, 
//         string modelName, 
//         CancellationToken ct = default);

//     // ─────────────────────────────────────────────────────────────
//     // Batch Operations (for background jobs)
//     // ─────────────────────────────────────────────────────────────
    
//     /// <summary>
//     /// Returns event IDs that don't have an embedding for the specified model.
//     /// Used by background job to find events needing embedding generation.
//     /// </summary>
//     Task<List<Guid>> GetUnembeddedEventIdsAsync(
//         string modelName,
//         int batchSize, 
//         CancellationToken ct = default);

//     /// <summary>
//     /// Returns embeddings that need re-embedding (event content changed).
//     /// </summary>
//     Task<List<EventEmbedding>> GetStaleEmbeddingsAsync(
//         string modelName,
//         DateTime contentChangedAfter,
//         CancellationToken ct = default);

//     // ─────────────────────────────────────────────────────────────
//     // ⭐ Vector Similarity Search (pgvector)
//     // ─────────────────────────────────────────────────────────────
    
//     /// <summary>
//     /// Cosine similarity search — returns top K event IDs closest to the query vector.
//     /// Uses pgvector HNSW index for fast approximate search.
//     /// Distance: lower = more similar (cosine distance: 0 = identical, 2 = opposite)
//     /// </summary>
//     Task<List<(Guid EventId, double Distance, string ModelName)>> SearchSimilarAsync(
//         float[] queryEmbedding,
//         int topK = 20,
//         float? maxDistance = null,
//         CancellationToken ct = default);

//     /// <summary>
//     /// Hybrid search: vector similarity + SQL filters (categories, active status, etc.)
//     /// </summary>
//     Task<List<(Guid EventId, double Distance, string ModelName)>> SearchSimilarWithFiltersAsync(
//         float[] queryEmbedding,
//         IEnumerable<string>? categories = null,
//         bool? isActive = true,
//         DateTime? minUpdatedAt = null,
//         int topK = 20,
//         float? maxDistance = null,
//         CancellationToken ct = default);

//     // ─────────────────────────────────────────────────────────────
//     // Analytics
//     // ─────────────────────────────────────────────────────────────
    
//     Task<int> GetCountByModelAsync(string modelName, CancellationToken ct = default);
//     Task<DateTime?> GetLatestEmbeddedAtAsync(string modelName, CancellationToken ct = default);
// }