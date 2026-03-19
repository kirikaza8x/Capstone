using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

/// <summary>
/// Repository for InteractionWeight.
/// CONFIGURATION: Read-Heavy, Cacheable.
/// Use case: "Get me the global weight for a 'click'" during scoring.
/// </summary>
public interface IInteractionWeightRepository : IRepository<InteractionWeight, Guid>
{
    /// <summary>
    /// Gets the active weight for a specific action type and version.
    /// Returns null if not found or inactive.
    /// </summary>
    Task<InteractionWeight?> GetByActionTypeAsync(
        string actionType,
        string version = "default",
        CancellationToken ct = default);

    /// <summary>
    /// Gets all active weights (for caching or A/B testing selection).
    /// </summary>
    Task<List<InteractionWeight>> GetActiveWeightsAsync(
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Versioning / A/B Testing
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all versions of weights for an action type (for A/B test selection).
    /// </summary>
    Task<List<InteractionWeight>> GetAllVersionsForActionAsync(
        string actionType,
        CancellationToken ct = default);

    /// <summary>
    /// Activates a specific version and deactivates others for an action type.
    /// Ensures only one active version per action at a time.
    /// </summary>
    Task ActivateVersionAsync(
        string actionType,
        string versionToActivate,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Configuration Management
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates a weight (UPSERT pattern for config).
    /// </summary>
    Task<InteractionWeight> UpsertAsync(
        string actionType,
        double weight,
        string? description = null,
        string version = "default",
        CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes (deactivates) a weight.
    /// </summary>
    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Analytics
    // ─────────────────────────────────────────────────────────────

    Task<int> GetActiveCountAsync(CancellationToken ct = default);
    Task<Dictionary<string, double>> GetAllActiveWeightsAsDictionaryAsync(
        CancellationToken ct = default);
}