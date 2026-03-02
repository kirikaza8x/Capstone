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

        // Implementation of the interface method you provided
        public async Task<UserWeightProfile?> GetAsync(Guid userId, string actionType)
        {
            // NOTE: Even though the interface asks for 'actionType', 
            // we fetch the whole profile because 'CustomWeights' is stored as a JSON blob.
            // The logic to find the specific "click" or "view" weight happens in C# memory
            // inside your Calculator service, not in this SQL query.

            return await _aiDbContext.UserWeightProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}