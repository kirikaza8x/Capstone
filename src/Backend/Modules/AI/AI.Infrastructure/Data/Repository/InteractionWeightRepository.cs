using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data; // Ensure this points to your AIDbContext

namespace AI.Infrastructure.Repositories
{
    public class InteractionWeightRepository : RepositoryBase<InteractionWeight, Guid>, IInteractionWeightRepository
    {
        private readonly AIModuleDbContext _aiDbContext;
        private readonly DbSet<InteractionWeight> _dbSet;


        public InteractionWeightRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _aiDbContext = dbContext;
            _dbSet = dbContext.Set<InteractionWeight>();
        }

        public async Task<InteractionWeight?> GetByActionTypeAsync(string actionType)
        {
            // Case-insensitive comparison is usually safer
            return await _dbSet
                .FirstOrDefaultAsync(x => x.ActionType.ToLower() == actionType.ToLower());
        }

         public async Task<List<InteractionWeight>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<List<InteractionWeight>> GetActiveWeightsAsync()
        {
            return await _dbSet
                .Where(x => x.IsActive)
                .ToListAsync();
        }
    }
}