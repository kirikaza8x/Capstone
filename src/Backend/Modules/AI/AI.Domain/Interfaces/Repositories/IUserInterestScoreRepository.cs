using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository for UserInterestScore with optimized batch operations
    /// </summary>
    public interface IUserInterestScoreRepository : IRepository<UserInterestScore, Guid>
    {
        // ===== BASIC CRUD =====
        Task<UserInterestScore?> GetByIdAsync(Guid id);
        Task<UserInterestScore?> GetByUserAndCategoryAsync(Guid userId, string category);
        Task<List<UserInterestScore>> GetAllForUserAsync(Guid userId);
        void Delete(UserInterestScore entity);

        // ===== OPTIMIZATION: Batch Operations =====
        /// <summary>
        /// Fetches multiple categories for a user in a single DB call
        /// </summary>
        Task<List<UserInterestScore>> GetByUserAndCategoriesAsync(Guid userId, List<string> categories);

        /// <summary>
        /// Thread-safe UPSERT operation to prevent race conditions.
        /// Returns the existing or newly created entity.
        /// </summary>
        Task<UserInterestScore> UpsertAsync(Guid userId, string category, double weight, double halfLifeDays);

        /// <summary>
        /// Gets top N categories for a user ordered by score
        /// </summary>
        Task<List<UserInterestScore>> GetTopCategoriesAsync(Guid userId, int topN = 10);


        // ===== ANALYTICS QUERIES =====
        /// <summary>
        /// Gets all scores that haven't been updated in X days (for cleanup)
        /// </summary>
        Task<List<UserInterestScore>> GetStaleScoresAsync(int daysThreshold = 90);

        /// <summary>
        /// Gets the total number of tracked interests for a user
        /// </summary>
        Task<int> GetInterestCountAsync(Guid userId);
    }
}