using AI.Application.Abstractions.Qdrant.Model;

namespace AI.Application.Abstractions.Qdrant;

public interface IEventVectorRepository
{
    Task EnsureCollectionAsync(CancellationToken ct = default);

    Task UpsertEventAsync(
        EventVectorPayload evt,
        float[] embedding,
        CancellationToken ct = default);

    Task UpsertBatchAsync(
        IEnumerable<(EventVectorPayload Event, float[] Embedding)> items,
        CancellationToken ct = default);

    /// <summary>
    /// Semantic similarity search. Only call this with a real query vector
    /// (e.g. WeightedCentroid built from user behavior embeddings).
    /// Do NOT pass a zero vector — use ScrollByFilterAsync instead.
    /// </summary>
    Task<IReadOnlyList<EventSearchResult>> SearchSimilarAsync(
        float[] queryEmbedding,
        int limit = 20,
        float scoreThreshold = 0.3f,
        IReadOnlyList<string>? filterCategories = null,
        IReadOnlyList<string>? filterHashtags = null,
        DateTime? afterDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Filter-only retrieval with no query vector — use for category fallback
    /// and cold-start paths where no user interest vector is available.
    /// Returns Score = 0 for all results (no similarity computed).
    /// </summary>
    Task<IReadOnlyList<EventSearchResult>> ScrollByFilterAsync(
        int limit,
        IReadOnlyList<string>? filterCategories = null,
        IReadOnlyList<string>? filterHashtags = null,
        DateTime? afterDate = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid eventId, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetAllEventIdsAsync(CancellationToken ct = default);
}