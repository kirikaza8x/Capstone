using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for GlobalCategoryStat.
    /// Updated by the background decay job — not by user-facing commands.
    /// Read path is used for cold-start recommendations and Bayesian smoothing.
    /// </summary>
    public interface IGlobalCategoryStatRepository : IRepository<GlobalCategoryStat, Guid>
    {
        // ===== SINGLE LOOKUPS =====

        Task<GlobalCategoryStat?> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        // ===== READ (recommendation hot path) =====

        /// <summary>
        /// Returns the N most popular categories by PopularityScore.
        /// Primary cold-start fallback when a user has no interest scores.
        /// </summary>
        Task<List<GlobalCategoryStat>> GetTopCategoriesAsync(
            int topN = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns stats for a specific set of categories in a single DB call.
        /// Used by the Bayesian smoother to blend personal and global scores.
        /// </summary>
        Task<List<GlobalCategoryStat>> GetByCategoriesAsync(
            IEnumerable<string> categories,
            CancellationToken cancellationToken = default);


        /// <summary>
        /// INSERT or UPDATE based on Category uniqueness.
        /// Always use instead of Add/Update to avoid duplicate-key errors.
        /// </summary>
        Task UpsertAsync(
            GlobalCategoryStat stat,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments activity counters for a category without loading the full entity.
        /// Called by the domain service on every behavior log event.
        /// Creates the record if it does not yet exist.
        /// </summary>
        Task IncrementActivityAsync(
            string category,
            double scoreIncrement,
            int interactionIncrement = 1,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies the same decay factor to all category stats in a single batch update.
        /// Called by the nightly decay job — more efficient than loading all entities.
        /// Returns the number of records updated.
        /// </summary>
        Task<int> ApplyDecayToAllAsync(
            double decayFactor,
            CancellationToken cancellationToken = default);

        // ===== CLEANUP =====

        /// <summary>
        /// Returns stats that satisfy GlobalCategoryStat.IsStale() —
        /// no recent activity and score below threshold for longer than daysThreshold.
        /// Uses RecentInteractions (not cumulative TotalInteractions) for the check.
        /// </summary>
        Task<List<GlobalCategoryStat>> GetStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Hard-deletes stale category stats. Returns count of deleted rows.
        /// </summary>
        Task<int> DeleteStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default);
    }
}