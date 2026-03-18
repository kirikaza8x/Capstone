namespace AI.Application.Abstractions;

/// <summary>
/// Shared re-indexing logic used by both the background job and manual endpoint.
/// </summary>
public interface IEventReIndexService
{
    /// <summary>
    /// Re-index all active events from Events module into Qdrant.
    /// Upserts — safe to run multiple times, existing vectors are overwritten.
    /// Returns count of events processed.
    /// </summary>
    Task<int> ReIndexAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Re-index a single event by ID.
    /// </summary>
    Task ReIndexOneAsync(Guid eventId, CancellationToken ct = default);
}