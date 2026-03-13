using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Domain.ValueObjects;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Repositories
{
    public class UserBehaviorLogRepository : RepositoryBase<UserBehaviorLog, Guid>, IUserBehaviorLogRepository
    {
        private readonly AIModuleDbContext _context;
        private readonly DbSet<UserBehaviorLog> _dbSet;

        public UserBehaviorLogRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
            _dbSet = dbContext.Set<UserBehaviorLog>();
        }

        // ===== SINGLE LOOKUPS =====



        // ===== USER-SCOPED QUERIES =====

        public async Task<List<UserBehaviorLog>> GetByUserAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OccurredAt >= from && x.OccurredAt <= to)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserBehaviorLog>> GetRecentByUserAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.OccurredAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserBehaviorLog>> GetByTargetAsync(string targetId, string targetType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.TargetId == targetId && x.TargetType == targetType.ToLowerInvariant())
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        // ===== ACTION-FILTERED QUERIES =====

        public async Task<List<UserBehaviorLog>> GetConversionsAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var conversions = ActionTypes.Conversions.ToList();

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId
                         && x.OccurredAt >= from
                         && x.OccurredAt <= to
                         && conversions.Contains(x.ActionType))
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserBehaviorLog>> GetByActionTypesAsync(Guid userId, IEnumerable<string> actionTypes, CancellationToken cancellationToken = default)
        {
            var normalised = actionTypes.Select(a => a.ToLowerInvariant()).ToList();

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && normalised.Contains(x.ActionType))
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        // ===== PIPELINE QUERIES =====

        public async Task<List<UserBehaviorLog>> GetUnprocessedAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OccurredAt > since)
                .OrderBy(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Guid>> GetActiveUserIdsSinceAsync(DateTime since, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.OccurredAt > since)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        // ===== ANALYTICS =====

        public async Task<Dictionary<string, int>> GetActionTypeCountsAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OccurredAt >= from && x.OccurredAt <= to)
                .GroupBy(x => x.ActionType)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetCategoryCountsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            // Categories live in the JSON metadata column — must be materialised before parsing.
            // For high-volume tables consider a denormalised categories column instead.
            var logs = await _dbSet
                .AsNoTracking()
                .Where(x => x.OccurredAt >= from && x.OccurredAt <= to)
                .ToListAsync(cancellationToken);

            return logs
                .SelectMany(l => l.GetCategories())
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // ===== CLEANUP =====

        public async Task<int> PurgeOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.OccurredAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}