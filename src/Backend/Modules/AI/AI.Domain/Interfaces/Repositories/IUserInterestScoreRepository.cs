using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for UserInterestScore.
    /// The UPSERT pattern is the primary write path — call UpsertAsync from the
    /// domain service instead of Add/Update directly to avoid race conditions.
    /// </summary>
    public interface IUserInterestScoreRepository : IRepository<UserInterestScore, Guid>
    {
        // ===== SINGLE LOOKUPS =====

        Task<UserInterestScore?> GetByUserAndCategoryAsync(
            Guid userId,
            string category,
            CancellationToken cancellationToken = default);

        // ===== USER-SCOPED QUERIES =====

        Task<List<UserInterestScore>> GetAllForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches multiple specific categories for a user in a single DB call.
        /// Used by the embedding rebuild job to fetch only the categories it needs.
        /// </summary>
        Task<List<UserInterestScore>> GetByUserAndCategoriesAsync(
            Guid userId,
            IEnumerable<string> categories,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the top N categories by current (post-decay) score for a user.
        /// Primary input for UserEmbedding.Recalculate() weight dictionary.
        /// </summary>
        Task<List<UserInterestScore>> GetTopCategoriesAsync(
            Guid userId,
            int topN = 10,
            CancellationToken cancellationToken = default);

        // ===== WRITE =====

        /// <summary>
        /// Thread-safe UPSERT: fetches or creates the score, applies decay, adds points.
        /// Handles the race condition where two events arrive for the same (userId, category)
        /// simultaneously by using a DB-level lock or optimistic concurrency.
        ///
        /// Implementation must call score.DecayAndAdd(points, halfLifeDays) — not raw SQL —
        /// so domain logic and events are always invoked.
        /// </summary>
        Task<UserInterestScore> UpsertAsync(
            Guid userId,
            string category,
            double points,
            double halfLifeDays,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies decay to all scores for a user without adding new points.
        /// Used when the scoring job runs on a user who has had no new interactions.
        /// </summary>
        Task DecayAllForUserAsync(
            Guid userId,
            double halfLifeDays,
            CancellationToken cancellationToken = default);

        // ===== ANALYTICS =====

        Task<int> GetInterestCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        // ===== CLEANUP =====

        /// <summary>
        /// Returns scores that satisfy UserInterestScore.IsStale() — score near zero
        /// and not updated within daysThreshold. Safe to archive or delete.
        /// </summary>
        Task<List<UserInterestScore>> GetStaleScoresAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk-deletes stale scores. Returns count of deleted rows.
        /// Call only after confirming with GetStaleScoresAsync().
        /// </summary>
        Task<int> DeleteStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default);
    }
}