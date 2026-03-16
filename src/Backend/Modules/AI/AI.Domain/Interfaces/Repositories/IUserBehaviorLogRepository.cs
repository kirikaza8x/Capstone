using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

/// <summary>
/// Repository for UserBehaviorLog.
/// Primary use: append-only logging + background job queries for enrichment.
/// </summary>
public interface IUserBehaviorLogRepository : IRepository<UserBehaviorLog, Guid>
{
    // ─────────────────────────────────────────────────────────────
    // Basic Lookups
    // ─────────────────────────────────────────────────────────────
    
    // ─────────────────────────────────────────────────────────────
    // User-Centric Queries
    // ─────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Fetches all raw logs for a user since a specific time.
    /// Used by enrichment pipeline to rebuild user embeddings.
    /// </summary>
    Task<List<UserBehaviorLog>> GetByUserIdSinceAsync(
        Guid userId,
        DateTime since,
        string? targetType = null,
        int limit = 200,
        CancellationToken ct = default);

    /// <summary>
    /// Gets distinct categories a user has interacted with (extracted from metadata JSON).
    /// </summary>
    Task<List<string>> GetDistinctCategoriesForUserAsync(
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent interaction timestamp for a user.
    /// </summary>
    Task<DateTime?> GetLatestOccurrenceForUserAsync(
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Counts interactions by action type for a user.
    /// </summary>
    Task<Dictionary<string, int>> GetActionCountsForUserAsync(
        Guid userId,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Event-Centric Queries (for embedding pipeline)
    // ─────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Gets all logs targeting a specific event.
    /// Used to measure event popularity / engagement signals.
    /// </summary>
    Task<List<UserBehaviorLog>> GetLogsForEventAsync(
        Guid eventId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets logs grouped by target event for batch processing.
    /// </summary>
    Task<Dictionary<Guid, List<UserBehaviorLog>>> GetLogsGroupedByEventAsync(
        IEnumerable<Guid> eventIds,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Background Job Queries
    // ─────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Fetches all raw logs since a specific time (e.g., Last 24 Hours).
    /// We need raw logs because "category" is inside Metadata JSON,
    /// so grouping must happen in C# memory, not SQL.
    /// </summary>
    Task<List<UserBehaviorLog>> GetLogsSinceAsync(
        DateTime since,
        string? targetType = null,
        int limit = 1000,
        CancellationToken ct = default);

    /// <summary>
    /// Gets logs that haven't been processed by enrichment yet.
    /// (Requires an "IsProcessed" flag or processed_at timestamp column)
    /// </summary>
    Task<List<UserBehaviorLog>> GetUnprocessedLogsAsync(
        int batchSize,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Analytics
    // ─────────────────────────────────────────────────────────────
    
    Task<long> GetTotalCountAsync(CancellationToken ct = default);
    Task<long> GetCountByActionTypeAsync(string actionType, CancellationToken ct = default);
    Task<long> GetCountByUserAsync(Guid userId, CancellationToken ct = default);
}