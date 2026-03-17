namespace Shared.Application.Abstractions.Qdrant;

/// <summary>
/// Defines the raw, public-facing Qdrant operations.
/// </summary>
public interface IQdrantRepository
{
    Task EnsureCollectionAsync(CancellationToken ct = default);
    Task<float[]?> GetVectorAsync(Guid pointId, CancellationToken ct = default);
    Task DeleteAsync(Guid pointId, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}