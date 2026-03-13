using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore; // Required for distance extension methods

namespace AI.Infrastructure.Repositories
{
    public class UserEmbeddingRepository : RepositoryBase<UserEmbedding, Guid>, IUserEmbeddingRepository
    {
        private readonly AIModuleDbContext _dbContext;
        private readonly DbSet<UserEmbedding> _dbSet;

        public UserEmbeddingRepository(AIModuleDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<UserEmbedding>();
        }

        // ===== SINGLE LOOKUPS =====

        public async Task<UserEmbedding?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        // ===== SIMILARITY SEARCH =====

        public async Task<List<UserEmbedding>> FindSimilarUsersAsync(
            float[] embedding,
            int topN = 10,
            double minSimilarity = 0.0,
            CancellationToken cancellationToken = default)
        {
            var queryVector = new Vector(embedding);

            return await _dbSet
                .AsNoTracking()
                .Where(ue => ue.IsActive && !ue.IsStale)
                .OrderBy(ue => ((Vector)(object)ue.Embedding!).CosineDistance(queryVector))
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserEmbedding>> FindSimilarUsersToUserAsync(
            Guid userId,
            int topN = 10,
            double minSimilarity = 0.0,
            CancellationToken cancellationToken = default)
        {
            var userEmbedding = await GetByUserIdAsync(userId, cancellationToken);
            if (userEmbedding == null || userEmbedding.Embedding == null)
                return new List<UserEmbedding>();

            var queryVector = new Vector(userEmbedding.Embedding);

            return await _dbSet
                .AsNoTracking()
                .Where(ue => ue.UserId != userId && ue.IsActive && !ue.IsStale)
                .OrderBy(ue => ((Vector)(object)ue.Embedding!).CosineDistance(queryVector))
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        // ===== BULK QUERIES =====

        public async Task<List<UserEmbedding>> GetStaleEmbeddingsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.IsStale)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserEmbedding>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids.ToList();

            if (!idList.Any())
                return new List<UserEmbedding>();

            return await _dbSet
                .AsNoTracking()
                .Where(x => idList.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserEmbedding>> GetAllEmbeddingsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // ===== ANALYTICS =====

        public async Task<int> GetLowConfidenceCountAsync(
            double threshold = 0.5,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(x => x.Confidence < threshold, cancellationToken);
        }

        public async Task<List<UserEmbedding>> GetNotCalculatedSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.LastCalculated < since)
                .ToListAsync(cancellationToken);
        }

        // ===== CLEANUP =====

        public async Task<List<UserEmbedding>> GetArchivableAsync(
            int daysThreshold = 180,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.IsStale && x.LastCalculated < cutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertAsync(
            UserEmbedding embedding,
            CancellationToken cancellationToken = default)
        {
            var existing = await _dbSet
                .FirstOrDefaultAsync(
                    x => x.UserId == embedding.UserId,
                    cancellationToken);

            if (existing == null)
            {
                _dbSet.Add(embedding);
                return;
            }

            existing.UpdateEmbedding(embedding.Embedding);
        }
    }
}