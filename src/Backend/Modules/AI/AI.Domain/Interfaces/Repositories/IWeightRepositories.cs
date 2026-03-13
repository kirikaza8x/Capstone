using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for InteractionWeight.
    /// Weights are read far more often than they are written — implementations
    /// should cache the active weight set aggressively (e.g. in-memory with a short TTL).
    /// </summary>
    public interface IInteractionWeightRepository : IRepository<InteractionWeight, Guid>
    {

        /// <summary>
        /// Returns the active weight for a given action type and version.
        /// Returns null if no active weight exists — callers should fall back to a default.
        /// </summary>
        Task<InteractionWeight?> GetActiveAsync(
            string actionType,
            string version = "default",
            CancellationToken cancellationToken = default);

        // ===== BULK READ (hot path) =====

        /// <summary>
        /// Returns all active weights as a lookup dictionary keyed by ActionType.
        /// This is the primary read path — called on every behavior log processed.
        /// Implementations should cache this result.
        /// </summary>
        Task<Dictionary<string, double>> GetAllActiveWeightsAsync(
            string version = "default",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all weight records for a given action type across all versions.
        /// Used by the A/B test management UI to show version history.
        /// </summary>
        Task<List<InteractionWeight>> GetAllVersionsAsync(
            string actionType,
            CancellationToken cancellationToken = default);

        // ===== VERSION MANAGEMENT =====

        /// <summary>
        /// Returns all active weights for a specific version label.
        /// Used to preview a new weight set before making it the default.
        /// </summary>
        Task<List<InteractionWeight>> GetByVersionAsync(
            string version,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates all currently active weights for the given version.
        /// Call before activating a replacement to prevent duplicate active weights.
        /// Returns the number of records deactivated.
        /// </summary>
        Task<int> DeactivateVersionAsync(
            string version,
            CancellationToken cancellationToken = default);
    }
}