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

    Task<IReadOnlyList<EventSearchResult>> SearchSimilarAsync(
        float[] queryEmbedding,
        int limit = 20,
        float scoreThreshold = 0.3f,
        IReadOnlyList<string>? filterCategories = null,
        IReadOnlyList<string>? filterHashtags = null,
        DateTime? afterDate = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid eventId, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
