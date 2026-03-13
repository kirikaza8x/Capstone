using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Repositories
{
    public class UserWeightProfileRepository : RepositoryBase<UserWeightProfile, Guid>, IUserWeightProfileRepository
    {
        private readonly AIModuleDbContext _aiDbContext;

        public UserWeightProfileRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _aiDbContext = dbContext;
        }

        /// <summary>
        /// Fetches the user's full weight profile.
        /// CustomWeights is stored as a JSON blob — the specific actionType lookup
        /// happens in the calling service after materialising the entity, not in SQL.
        /// </summary>
        public async Task<UserWeightProfile?> GetAsync(Guid userId, string actionType)
        {
            return await _aiDbContext.UserWeightProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}