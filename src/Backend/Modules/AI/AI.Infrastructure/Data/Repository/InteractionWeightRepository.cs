using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data; // Ensure this points to your AIDbContext

namespace AI.Infrastructure.Repositories
{
    public class InteractionWeightRepository : RepositoryBase<InteractionWeight, Guid>, IInteractionWeightRepository
    {
        // We cast the generic context to our specific AIDbContext to access specific DbSets if needed
        private readonly AIModuleDbContext _aiDbContext;

        public InteractionWeightRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _aiDbContext = dbContext;
        }

        public async Task<InteractionWeight?> GetByActionTypeAsync(string actionType)
        {
            // Case-insensitive comparison is usually safer
            return await _aiDbContext.InteractionWeights
                .FirstOrDefaultAsync(x => x.ActionType.ToLower() == actionType.ToLower());
        }
    }
}