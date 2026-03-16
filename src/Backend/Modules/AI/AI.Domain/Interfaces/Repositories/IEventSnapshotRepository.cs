using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

public interface IEventSnapshotRepository : IRepository<EventSnapshot, Guid>
{
    // ─────────────────────────────────────────────────────────────
    // Basic Lookups
    // ─────────────────────────────────────────────────────────────
    
    Task<EventSnapshot?> GetByEventIdAsync(
        Guid eventId, 
        CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Query by Content
    // ─────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Find snapshots by category membership (JSONB containment: categories @> ["music"])
    /// </summary>
    Task<List<EventSnapshot>> GetActiveByCategoriesAsync(
        IEnumerable<string> categories,
        int limit = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Find snapshots by hashtag membership
    /// </summary>
    Task<List<EventSnapshot>> GetActiveByHashtagsAsync(
        IEnumerable<string> hashtags,
        int limit = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Full-text style search on title + description (simple LIKE for now)
    /// For production: add tsvector column + GIN index
    /// </summary>
    Task<List<EventSnapshot>> SearchByTextAsync(
        string query,
        int limit = 50,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Batch Operations (for embedding pipeline)
    // ─────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Returns event IDs that exist as snapshots but don't have embeddings.
    /// Used to find events needing initial embedding generation.
    /// </summary>
    Task<List<Guid>> GetUnembeddedEventIdsAsync(
        string embeddingModelName,
        int batchSize, 
        CancellationToken ct = default);

    /// <summary>
    /// Returns snapshots that were updated after their embedding was generated.
    /// Used to find events needing re-embedding.
    /// </summary>
    Task<List<EventSnapshot>> GetChangedSinceEmbeddingAsync(
        string embeddingModelName,
        int batchSize,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────
    
    Task<List<EventSnapshot>> GetActiveAsync(
        DateTime? minUpdatedAt = null,
        int limit = 100,
        CancellationToken ct = default);

    Task DeactivateAsync(Guid eventId, CancellationToken ct = default);
}