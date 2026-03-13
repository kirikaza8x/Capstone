using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Repositories
{
    public class CategoryCoOccurrenceRepository : RepositoryBase<CategoryCoOccurrence, Guid>, ICategoryCoOccurrenceRepository
    {
        private readonly AIModuleDbContext _dbContext;
        private readonly DbSet<CategoryCoOccurrence> _dbSet;

        public CategoryCoOccurrenceRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<CategoryCoOccurrence>();
        }

        // ===== SINGLE LOOKUPS =====

        public async Task<CategoryCoOccurrence?> GetByCategoryPairAsync(
            string category1,
            string category2,
            CancellationToken cancellationToken = default)
        {
            // Enforce consistent ordering to match Create() logic
            if (string.Compare(category1, category2, StringComparison.Ordinal) > 0)
            {
                (category1, category2) = (category2, category1);
            }

            var cat1 = category1.ToLowerInvariant().Trim();
            var cat2 = category2.ToLowerInvariant().Trim();

            return await _dbSet
                .FirstOrDefaultAsync(x => x.Category1 == cat1 && x.Category2 == cat2, cancellationToken);
        }

        public async Task<List<CategoryCoOccurrence>> GetByCategoryPairsAsync(
            IEnumerable<(string Category1, string Category2)> pairs,
            CancellationToken cancellationToken = default)
        {
            var pairList = pairs.ToList();

            if (!pairList.Any())
                return new List<CategoryCoOccurrence>();

            // Normalise all pairs with consistent ordering
            var normalisedPairs = pairList.Select(p =>
                string.Compare(p.Category1, p.Category2, StringComparison.Ordinal) > 0
                    ? (Category1: p.Category2.ToLowerInvariant().Trim(), Category2: p.Category1.ToLowerInvariant().Trim())
                    : (Category1: p.Category1.ToLowerInvariant().Trim(), Category2: p.Category2.ToLowerInvariant().Trim())
            ).ToList();

            var query = _dbSet.AsNoTracking();

            // Build a dynamic OR query for all pairs
            var results = new List<CategoryCoOccurrence>();
            foreach (var pair in normalisedPairs)
            {
                var match = await query
                    .FirstOrDefaultAsync(x => x.Category1 == pair.Category1 && x.Category2 == pair.Category2, cancellationToken);
                if (match != null)
                    results.Add(match);
            }

            return results;
        }

        // ===== CATEGORY-SCOPED QUERIES =====

        public async Task<List<CategoryCoOccurrence>> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            var normalised = category.ToLowerInvariant().Trim();

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Category1 == normalised || x.Category2 == normalised)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryCoOccurrence>> GetTopRelatedAsync(
            string category,
            int topN = 10,
            double minLiftScore = 1.0,
            CancellationToken cancellationToken = default)
        {
            var normalised = category.ToLowerInvariant().Trim();

            return await _dbSet
                .AsNoTracking()
                .Where(x => (x.Category1 == normalised || x.Category2 == normalised) && x.LiftScore >= minLiftScore)
                .OrderByDescending(x => x.LiftScore)
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryCoOccurrence>> GetTopByCoOccurrenceAsync(
            string category,
            int topN = 10,
            int minCount = 2,
            CancellationToken cancellationToken = default)
        {
            var normalised = category.ToLowerInvariant().Trim();

            return await _dbSet
                .AsNoTracking()
                .Where(x => (x.Category1 == normalised || x.Category2 == normalised) && x.CoOccurrenceCount >= minCount)
                .OrderByDescending(x => x.CoOccurrenceCount)
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        // ===== BULK QUERIES =====

        public async Task<List<CategoryCoOccurrence>> GetHighLiftAsync(
            double minLiftScore = 1.5,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.LiftScore >= minLiftScore)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryCoOccurrence>> GetNotUpdatedSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.LastUpdated < since)
                .ToListAsync(cancellationToken);
        }

        // ===== ANALYTICS =====

        public async Task<int> GetTotalCountAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }

        public async Task<double> GetAverageLiftScoreAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AverageAsync(x => (double?)x.LiftScore, cancellationToken) ?? 0.0;
        }

        // ===== CLEANUP =====

        public async Task<List<CategoryCoOccurrence>> GetStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.LastUpdated < cutoff && x.CoOccurrenceCount < 2)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> DeleteStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .Where(x => x.LastUpdated < cutoff && x.CoOccurrenceCount < 2)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
