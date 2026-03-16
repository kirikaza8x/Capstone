namespace Shared.Application.Abstractions.Qdrant;

/// <summary>
/// Shared contract for all Qdrant repositories.
/// Contains only operations that every collection needs regardless of domain.
/// </summary>
public interface IQdrantRepositoryBase
{
    /// <summary>Creates the collection if it does not already exist.</summary>
    Task EnsureCollectionAsync(CancellationToken ct = default);

    /// <summary>Returns the stored vector for a point, or null if not found.</summary>
    Task<float[]?> GetEmbeddingAsync(Guid pointId, CancellationToken ct = default);

    /// <summary>Removes a single point by id.</summary>
    Task DeleteAsync(Guid pointId, CancellationToken ct = default);

    /// <summary>Returns the total number of points in the collection.</summary>
    Task<int> GetCountAsync(CancellationToken ct = default);
}