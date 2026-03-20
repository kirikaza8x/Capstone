using AI.Domain.ReadModels;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Repositories
{
    public class GlobalCategoryStatRepository : RepositoryBase<GlobalCategoryStat, Guid>, IGlobalCategoryStatRepository
    {
        private readonly AIModuleDbContext _dbContext;
        private readonly DbSet<GlobalCategoryStat> _dbSet;
        public GlobalCategoryStatRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<GlobalCategoryStat>();
        }

        public async Task<GlobalCategoryStat?> GetByCategoryAsync(string category)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Category == category.ToLower());
        }

        public async Task<List<GlobalCategoryStat>> GetByCategoryNamesAsync(List<string> categories)
        {
            var normalizedCategories = categories
                .Select(c => c.ToLowerInvariant())
                .ToList();

            return await _dbSet
                .Where(x => normalizedCategories.Contains(x.Category))
                .ToListAsync();
        }

        public async Task<List<GlobalCategoryStat>> GetTopCategoriesAsync(int topN = 20)
        {
            return await _dbSet
                .OrderByDescending(x => x.PopularityScore)
                .Take(topN)
                .ToListAsync();
        }

        public async Task<List<GlobalCategoryStat>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task ApplyGlobalDecayAsync(double decayFactor)
        {
            var allStats = await _dbSet.ToListAsync();

            foreach (var stat in allStats)
            {
                stat.ApplyDecay(decayFactor);
            }

            _dbSet.UpdateRange(allStats);
        }

        public async Task<int> GetTotalCategoriesAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<List<GlobalCategoryStat>> GetStaleStatsAsync(int daysThreshold = 90)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .Where(x =>
                    x.LastCalculated < cutoff &&
                    x.PopularityScore < 1.0 &&
                    x.TotalInteractions == 0)
                .ToListAsync();
        }
    }
}
