using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using AI.Domain.Entities;

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

        // ===== SINGLE LOOKUPS =====

        public async Task<GlobalCategoryStat?> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Category == category.ToLower(), cancellationToken);
        }

        // ===== READ =====

        public async Task<List<GlobalCategoryStat>> GetTopCategoriesAsync(
            int topN = 20,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .OrderByDescending(x => x.PopularityScore)
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GlobalCategoryStat>> GetByCategoriesAsync(
            IEnumerable<string> categories,
            CancellationToken cancellationToken = default)
        {
            var normalised = categories.Select(c => c.ToLowerInvariant()).ToList();

            if (!normalised.Any())
                return new List<GlobalCategoryStat>();

            return await _dbSet
                .Where(x => normalised.Contains(x.Category))
                .ToListAsync(cancellationToken);
        }

        // ===== WRITE =====

        public async Task UpsertAsync(
            GlobalCategoryStat stat,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbSet
                .AnyAsync(x => x.Category == stat.Category, cancellationToken);

            if (exists)
                _dbSet.Update(stat);
            else
                _dbSet.Add(stat);
        }

        public async Task IncrementActivityAsync(
            string category,
            double scoreIncrement,
            int interactionIncrement = 1,
            CancellationToken cancellationToken = default)
        {
            var normalised = category.ToLowerInvariant().Trim();
            var stat = await _dbSet
                .FirstOrDefaultAsync(x => x.Category == normalised, cancellationToken);

            if (stat is null)
            {
                stat = GlobalCategoryStat.Create(normalised, scoreIncrement, interactionIncrement);
                _dbSet.Add(stat);
            }
            else
            {
                stat.AddActivity(scoreIncrement, interactionIncrement);
                _dbSet.Update(stat);
            }
        }

        /// <summary>
        /// Applies decay to ALL stats in a single batch UPDATE — no entity loading.
        /// The ScoreFloor value (0.1) must stay in sync with GlobalCategoryStat.ScoreFloor.
        /// Returns number of rows updated.
        /// </summary>
        public async Task<int> ApplyDecayToAllAsync(
            double decayFactor,
            CancellationToken cancellationToken = default)
        {
            const double scoreFloor = 0.1;

            return await _dbSet
                .Where(x => x.PopularityScore > 0)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(
                        e => e.PopularityScore,
                        e => e.PopularityScore * decayFactor < scoreFloor
                            ? 0
                            : e.PopularityScore * decayFactor)
                    .SetProperty(
                        e => e.RawWeightedScore,
                        e => e.RawWeightedScore * decayFactor < scoreFloor
                            ? 0
                            : e.RawWeightedScore * decayFactor)
                    .SetProperty(e => e.LastCalculated, _ => DateTime.UtcNow),
                    cancellationToken);
        }

        // ===== CLEANUP =====

        /// <summary>
        /// Uses RecentInteractions == 0 (not cumulative TotalInteractions) so historically
        /// popular but currently quiet categories are correctly flagged as stale.
        /// </summary>
        public async Task<List<GlobalCategoryStat>> GetStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.LastCalculated < cutoff
                         && x.PopularityScore < 1.0
                         && x.RecentInteractions == 0)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> DeleteStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .Where(x => x.LastCalculated < cutoff
                         && x.PopularityScore < 1.0
                         && x.RecentInteractions == 0)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}