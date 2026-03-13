using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
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

        // ===== SINGLE LOOKUPS =====

        public async Task<UserInterestScore?> GetByUserAndCategoryAsync(
            Guid userId,
            string category,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(
                    x => x.UserId == userId
                      && x.Category == category.ToLowerInvariant().Trim(),
                    cancellationToken);
        }

        // ===== USER-SCOPED QUERIES =====

        public async Task<List<UserInterestScore>> GetAllForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Score)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserInterestScore>> GetByUserAndCategoriesAsync(
            Guid userId,
            IEnumerable<string> categories,
            CancellationToken cancellationToken = default)
        {
            var normalised = categories.Select(c => c.ToLowerInvariant().Trim()).ToList();

            if (!normalised.Any())
                return new List<UserInterestScore>();

            return await _dbSet
                .Where(x => x.UserId == userId && normalised.Contains(x.Category))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserInterestScore>> GetTopCategoriesAsync(
            Guid userId,
            int topN = 10,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        // ===== WRITE =====

        /// <summary>
        /// Thread-safe UPSERT. Always routes through DecayAndAdd() — never raw field assignment —
        /// so domain logic and events fire correctly.
        /// Handles the duplicate-key race condition via DB exception retry.
        /// </summary>
        public async Task<UserInterestScore> UpsertAsync(
            Guid userId,
            string category,
            double points,
            double halfLifeDays,
            CancellationToken cancellationToken = default)
        {
            var normalised = category.ToLowerInvariant().Trim();
            var existing = await GetByUserAndCategoryAsync(userId, normalised, cancellationToken);

            if (existing != null)
            {
                existing.DecayAndAdd(points, halfLifeDays);
                _dbSet.Update(existing);
                return existing;
            }

            var newScore = UserInterestScore.Create(userId, normalised);
            newScore.AddScore(points);

            try
            {
                _dbSet.Add(newScore);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return newScore;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // Race condition — another thread inserted between our read and write.
                _dbContext.Entry(newScore).State = EntityState.Detached;

                existing = await GetByUserAndCategoryAsync(userId, normalised, cancellationToken);
                if (existing != null)
                {
                    existing.DecayAndAdd(points, halfLifeDays);
                    _dbSet.Update(existing);
                    return existing;
                }

                throw;
            }
        }

        public async Task DecayAllForUserAsync(
            Guid userId,
            double halfLifeDays,
            CancellationToken cancellationToken = default)
        {
            var scores = await _dbSet
                .Where(x => x.UserId == userId && x.Score > 0)
                .ToListAsync(cancellationToken);

            foreach (var score in scores)
                score.ApplyDecay(halfLifeDays);

            _dbSet.UpdateRange(scores);
        }

        // ===== ANALYTICS =====

        public async Task<int> GetInterestCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(x => x.UserId == userId && x.Score > 0, cancellationToken);
        }

        // ===== CLEANUP =====

        public async Task<List<UserInterestScore>> GetStaleScoresAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.ModifiedAt < cutoff && x.Score < 1.0)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> DeleteStaleAsync(
            int daysThreshold = 90,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .Where(x => x.ModifiedAt < cutoff && x.Score < 1.0)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}