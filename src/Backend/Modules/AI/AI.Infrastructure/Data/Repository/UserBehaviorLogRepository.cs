using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Repositories
{
    public class UserBehaviorLogRepository : RepositoryBase<UserBehaviorLog, Guid>, IUserBehaviorLogRepository
    {
        public UserBehaviorLogRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
        }
    }
}