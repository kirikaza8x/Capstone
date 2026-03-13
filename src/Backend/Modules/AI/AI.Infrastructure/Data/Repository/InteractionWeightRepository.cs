using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;

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


        public async Task<InteractionWeight?> GetActiveAsync(
            string actionType,
            string version = "default",
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ActionType == actionType.ToLowerInvariant().Trim()
                      && x.Version == version
                      && x.IsActive,
                    cancellationToken);
        }

        // ===== BULK READ (hot path, cached) =====

        public async Task<Dictionary<string, double>> GetAllActiveWeightsAsync(
            string version = "default",
            CancellationToken cancellationToken = default)
        {



            var weights = await _dbSet
                .AsNoTracking()
                .Where(x => x.Version == version && x.IsActive)
                .ToDictionaryAsync(x => x.ActionType, x => x.Weight, cancellationToken);


            return weights;
        }

        public async Task<List<InteractionWeight>> GetAllVersionsAsync(
            string actionType,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.ActionType == actionType.ToLowerInvariant().Trim())
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // ===== VERSION MANAGEMENT =====

        public async Task<List<InteractionWeight>> GetByVersionAsync(
            string version,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Version == version)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> DeactivateVersionAsync(
            string version,
            CancellationToken cancellationToken = default)
        {
            var weights = await _dbSet
                .Where(x => x.Version == version && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var w in weights)
                w.Deactivate();

            _dbSet.UpdateRange(weights);

            return weights.Count;
        }
    }
}