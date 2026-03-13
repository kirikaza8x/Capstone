using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for CategoryCoOccurrence.
    /// Tracks category co-occurrence patterns for collaborative filtering.
    /// Used to build "users who liked X also liked Y" recommendations.
    /// </summary>
    public interface ICategoryCoOccurrenceRepository : IRepository<CategoryCoOccurrence, Guid>
    {
        // ===== SINGLE LOOKUPS =====

        /// <summary>
        /// Returns the co-occurrence record for a specific category pair.
        /// Handles ordering internally (A,B) vs (B,A) are treated as the same pair.
        /// </summary>
        Task<CategoryCoOccurrence?> GetByCategoryPairAsync(
            string category1,
            string category2,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns co-occurrence records for multiple category pairs.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetByCategoryPairsAsync(
            IEnumerable<(string Category1, string Category2)> pairs,
            CancellationToken cancellationToken = default);

        // ===== CATEGORY-SCOPED QUERIES =====

        /// <summary>
        /// Returns all co-occurrence records involving a specific category.
        /// Primary query for "users who liked X also liked..." recommendations.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the top N related categories for a given category based on lift score.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetTopRelatedAsync(
            string category,
            int topN = 10,
            double minLiftScore = 1.0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the top N related categories for a given category based on co-occurrence count.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetTopByCoOccurrenceAsync(
            string category,
            int topN = 10,
            int minCount = 2,
            CancellationToken cancellationToken = default);

        // ===== BULK QUERIES =====

        /// <summary>
        /// Returns all co-occurrence records with lift score above a threshold.
        /// Used for building the recommendation graph.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetHighLiftAsync(
            double minLiftScore = 1.5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all co-occurrence records that haven't been updated since the given timestamp.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetNotUpdatedSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default);

        // ===== ANALYTICS =====

        /// <summary>
        /// Returns the total count of co-occurrence records.
        /// </summary>
        Task<int> GetTotalCountAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the average lift score across all co-occurrence records.
        /// </summary>
        Task<double> GetAverageLiftScoreAsync(
            CancellationToken cancellationToken = default);

        // ===== CLEANUP =====

        /// <summary>
        /// Returns co-occurrence records that are stale and can be archived.
        /// Uses CategoryCoOccurrence.IsStale() logic.
        /// </summary>
        Task<List<CategoryCoOccurrence>> GetStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk-deletes stale co-occurrence records.
        /// Returns the number of rows deleted.
        /// </summary>
        Task<int> DeleteStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default);
    }
}
