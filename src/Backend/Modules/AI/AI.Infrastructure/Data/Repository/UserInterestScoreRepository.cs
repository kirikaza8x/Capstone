using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data; // Assuming this is where RepositoryBase is
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Repositories
{
    public class UserInterestScoreRepository : RepositoryBase<UserInterestScore, Guid>, IUserInterestScoreRepository
    {
        private readonly AIModuleDbContext _dbContext;
        private readonly DbSet<UserInterestScore> _dbSet;
        public UserInterestScoreRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<UserInterestScore>();
        }

        public async Task<UserInterestScore?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<UserInterestScore?> GetByUserAndCategoryAsync(Guid userId, string category)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => 
                    x.UserId == userId && 
                    x.Category == category.ToLower());
        }

        public async Task<List<UserInterestScore>> GetAllForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Score)
                .ToListAsync();
        }

        public void Delete(UserInterestScore entity)
        {
            _dbSet.Remove(entity);
        }

        // ===== OPTIMIZATION: Batch Operations =====

        /// <summary>
        /// CRITICAL: Fetches multiple categories in ONE database roundtrip.
        /// This is a massive performance win for multi-category actions.
        /// </summary>
        public async Task<List<UserInterestScore>> GetByUserAndCategoriesAsync(
            Guid userId, 
            List<string> categories)
        {
            if (!categories.Any())
                return new List<UserInterestScore>();

            // Normalize categories for comparison
            var normalizedCategories = categories
                .Select(c => c.ToLowerInvariant())
                .ToList();

            return await _dbSet
                .Where(x => 
                    x.UserId == userId && 
                    normalizedCategories.Contains(x.Category))
                .ToListAsync();
        }

        /// <summary>
        /// CRITICAL: Thread-safe UPSERT to prevent duplicate records.
        /// Uses database-level unique constraint + retry logic.
        /// </summary>
        public async Task<UserInterestScore> UpsertAsync(
            Guid userId, 
            string category, 
            double weight, 
            double halfLifeDays)
        {
            // Normalize the category
            string normalizedCategory = category.ToLowerInvariant().Trim();

            // Try to find existing record
            var existing = await GetByUserAndCategoryAsync(userId, normalizedCategory);

            if (existing != null)
            {
                // Update existing
                existing.ApplyDecay(halfLifeDays);
                existing.AddScore(weight);
                _dbSet.Update(existing);
                return existing;
            }

            // Create new record
            var newScore = UserInterestScore.Create(userId, normalizedCategory, weight);
            
            try
            {
                _dbSet.Add(newScore);
                await _dbContext.SaveChangesAsync();
                return newScore;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // Race condition detected - another thread created it
                // Refresh and try update instead
                _dbContext.Entry(newScore).State = EntityState.Detached;
                
                existing = await GetByUserAndCategoryAsync(userId, normalizedCategory);
                if (existing != null)
                {
                    existing.ApplyDecay(halfLifeDays);
                    existing.AddScore(weight);
                    _dbSet.Update(existing);
                    return existing;
                }
                
                throw; // Unexpected error
            }
        }

        public async Task<List<UserInterestScore>> GetTopCategoriesAsync(Guid userId, int topN = 10)
        {
            return await _dbSet
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .ToListAsync();
        }

        // ===== ANALYTICS QUERIES =====

        public async Task<List<UserInterestScore>> GetStaleScoresAsync(int daysThreshold = 90)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);
            
            return await _dbSet
                .Where(x => x.LastUpdated < cutoff && x.Score < 1.0)
                .ToListAsync();
        }

        public async Task<int> GetInterestCountAsync(Guid userId)
        {
            return await _dbSet
                .Where(x => x.UserId == userId)
                .CountAsync();
        }
    }
}