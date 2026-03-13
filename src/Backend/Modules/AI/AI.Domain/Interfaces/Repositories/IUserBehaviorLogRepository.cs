using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for UserBehaviorLog.
    /// WRITE-ONLY aggregate — no Update/Delete operations exposed by design.
    /// All queries are scoped by userId for privacy and performance.
    /// </summary>
    public interface IUserBehaviorLogRepository : IRepository<UserBehaviorLog, Guid>
    {

        // ===== USER-SCOPED QUERIES =====

        /// <summary>
        /// Returns logs for a user within a time window, newest first.
        /// Primary feed for the scoring pipeline.
        /// </summary>
        Task<List<UserBehaviorLog>> GetByUserAsync(
            Guid userId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the N most recent logs for a user regardless of time window.
        /// Used for incremental UserEmbedding rebuilds.
        /// </summary>
        Task<List<UserBehaviorLog>> GetRecentByUserAsync(
            Guid userId,
            int limit = 100,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns logs for a specific target (e.g. all users who viewed product-123).
        /// Used for item-based collaborative filtering.
        /// </summary>
        Task<List<UserBehaviorLog>> GetByTargetAsync(
            string targetId,
            string targetType,
            CancellationToken cancellationToken = default);

        // ===== ACTION-FILTERED QUERIES =====

        /// <summary>
        /// Returns only conversion events (purchase, subscribe, etc.) for a user.
        /// Faster than filtering in-memory when the table is large.
        /// </summary>
        Task<List<UserBehaviorLog>> GetConversionsAsync(
            Guid userId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns logs filtered to a specific set of action types.
        /// Useful when the scoring pipeline only needs high-value signals.
        /// </summary>
        Task<List<UserBehaviorLog>> GetByActionTypesAsync(
            Guid userId,
            IEnumerable<string> actionTypes,
            CancellationToken cancellationToken = default);

        // ===== BATCH / PIPELINE QUERIES =====

        /// <summary>
        /// Returns all logs for a user that have not yet been processed by the scoring job.
        /// Requires an IsProcessed flag or a watermark timestamp tracked externally.
        /// </summary>
        Task<List<UserBehaviorLog>> GetUnprocessedAsync(
            Guid userId,
            DateTime since,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns distinct userIds that have new logs since a given timestamp.
        /// Used by the background job to find which UserEmbeddings need rebuilding.
        /// </summary>
        Task<List<Guid>> GetActiveUserIdsSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default);

        // ===== ANALYTICS =====

        /// <summary>
        /// Returns the count of interactions per action type for a user within a window.
        /// </summary>
        Task<Dictionary<string, int>> GetActionTypeCountsAsync(
            Guid userId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the count of interactions per category for a user within a window.
        /// Used to build GlobalCategoryStat increments without loading all rows.
        /// </summary>
        Task<Dictionary<string, int>> GetCategoryCountsAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

        // ===== CLEANUP =====

        /// <summary>
        /// Hard-deletes logs older than the retention cutoff.
        /// Should be called by the data-retention background job, not user-facing code.
        /// Returns the number of rows deleted.
        /// </summary>
        Task<int> PurgeOlderThanAsync(
            DateTime cutoff,
            CancellationToken cancellationToken = default);
    }
}