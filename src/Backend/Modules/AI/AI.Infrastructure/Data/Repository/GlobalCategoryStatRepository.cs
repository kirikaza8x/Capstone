using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Domain.ReadModels;
using AI.Infrastructure.Data;

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

        // The interface likely inherits GetAllAsync from IRepository, 
        // but if you have custom logic (like ordering by popularity), add it here.
        public async Task<List<GlobalCategoryStat>> GetTopCategoriesAsync(int count)
        {
            return await _dbContext.Set<GlobalCategoryStat>()
                .OrderByDescending(x => x.PopularityScore)
                .Take(count)
                .ToListAsync();
        }

        public async Task<GlobalCategoryStat?> GetByCategoryAsync(string category)
        {
            return await _dbContext.Set<GlobalCategoryStat>()
                .FirstOrDefaultAsync(x => x.Category == category);
        }

        public async Task<List<GlobalCategoryStat>> GetAllAsync()
        {
            return await _dbContext.Set<GlobalCategoryStat>()
                .ToListAsync();
        }
    }
}