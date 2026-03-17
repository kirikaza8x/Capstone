using AI.Application.Abstractions.Qdrant.Model;

namespace AI.Application.Abstractions.Qdrant;

/// <summary>
/// Contract for storing and searching user behavior embeddings.
/// Implemented by UserBehaviorVectorRepository in AI.Infrastructure.
/// </summary>
public interface IUserBehaviorVectorRepository
{
    Task EnsureCollectionAsync(CancellationToken ct = default);
    Task UpsertBehaviorAsync(UserBehaviorVectorPayload log, float[] embedding, CancellationToken ct = default);
    Task<IReadOnlyList<BehaviorSearchResult>> SearchUserBehaviorAsync(
        Guid              userId,
        float[]           queryVector,
        int               limit          = 50,
        float             scoreThreshold = 0.3f,
        string?           filterAction   = null,
        DateTime?         afterDate      = null,
        CancellationToken ct             = default);
    Task<IReadOnlyList<BehaviorSearchResult>> SearchGlobalBehaviorAsync(
        float[]           queryVector,
        int               limit          = 100,
        float             scoreThreshold = 0.4f,
        string?           filterAction   = null,
        CancellationToken ct             = default);
    Task<IReadOnlyDictionary<string, int>> GetCategoryFrequencyAsync(
        Guid              userId,
        DateTime?         since        = null,
        string?           filterAction = null,
        int               sampleSize   = 200,
        CancellationToken ct           = default);
    Task DeleteAsync(Guid logId, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}