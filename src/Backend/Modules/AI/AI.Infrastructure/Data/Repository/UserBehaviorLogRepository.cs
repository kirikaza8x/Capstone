using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Infrastructure.Repositories
{
    public class UserBehaviorLogRepository : RepositoryBase<UserBehaviorLog, Guid>, IUserBehaviorLogRepository
    {
        private readonly AIModuleDbContext _context;
        public UserBehaviorLogRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<List<UserBehaviorLog>> GetLogsSinceAsync(DateTime since)
        {
            // 1. Filter by Date (Index usage is critical here!)
            return await _context.UserBehaviorLogs
                .AsNoTracking()
                .Where(log => log.OccurredAt >= since)
                .ToListAsync();
        }
    }
}