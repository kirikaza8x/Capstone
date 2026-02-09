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

        public async Task<List<UserInterestScore>> GetAllForUserAsync(Guid userId)
        {
            return await _dbContext.Set<UserInterestScore>()
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserInterestScore?> GetByUserAndCategoryAsync(Guid userId, string category)
        {
            return await _dbContext.Set<UserInterestScore>()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Category == category);
        }
    }
}