using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace AI.Infrastructure.Repositories;

public class UserEmbeddingRepository : RepositoryBase<UserEmbedding, Guid>, IUserEmbeddingRepository
{
    private readonly AIModuleDbContext _dbContext;
    private readonly DbSet<UserEmbedding> _dbSet;

    public UserEmbeddingRepository(AIModuleDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<UserEmbedding>();
    }

    // ─────────────────────────────────────────────────────────────
    // Single Lookups
    // ─────────────────────────────────────────────────────────────

    public async Task<UserEmbedding?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(ue => ue.UserId == userId && ue.IsActive, cancellationToken);
    }

    // ─────────────────────────────────────────────────────────────
    // ⭐ Similarity Search (In-Memory Cosine Similarity)
    // ─────────────────────────────────────────────────────────────

    // public async Task<List<(Guid UserId, double Similarity, int SharedCategories)>> FindSimilarUsersAsync(
    //     float[] queryEmbedding,
    //     int topN = 10,
    //     double minSimilarity = 0.0,
    //     CancellationToken cancellationToken = default)
    // {
    //     // Fetch active embeddings
    //     var allEmbeddings = await _dbSet
    //         .AsNoTracking()
    //         .Where(ue => ue.IsActive && ue.Embedding != null)
    //         .ToListAsync(cancellationToken);

    //     // Compute similarity in memory
    //     var results = new List<(Guid UserId, double Similarity, int SharedCategories)>();

    //     foreach (var ue in allEmbeddings)
    //     {
    //         if (ue.Embedding == null || ue.Embedding.Length != queryEmbedding.Length)
    //             continue;

    //         var similarity = ue.CosineSimilarity(queryEmbedding);

    //         if (similarity >= minSimilarity)
    //         {
    //             results.Add((ue.UserId, similarity, ue.ContributingCategories.Count));
    //         }
    //     }

    //     return results
    //         .OrderByDescending(x => x.Similarity)
    //         .Take(topN)
    //         .ToList();
    // }

    public async Task<List<(Guid UserId, double Similarity, int SharedCategories)>> FindSimilarUsersAsync(
    float[] queryEmbedding,
    int topN = 10,
    double minSimilarity = 0.0,
    CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
            throw new ArgumentException("Query embedding cannot be empty");

        var queryVector = new Vector(queryEmbedding);

        // ✅ Perform the math in SQL using pgvector
        var query = _dbSet
            .AsNoTracking()
            .Where(ue => ue.IsActive)
            .Select(ue => new
            {
                ue.UserId,

                Distance = new Vector(ue.Embedding).CosineDistance(queryVector),
                CategoryCount = ue.ContributingCategories.Count
            });

        var results = await query
            .Where(x => (1.0 - x.Distance) >= minSimilarity)
            .OrderBy(x => x.Distance)
            .Take(topN)
            .ToListAsync(cancellationToken);

        return results
            .Select(x => (x.UserId, 1.0 - x.Distance, x.CategoryCount))
            .ToList();
    }

    public async Task<List<(Guid UserId, double Similarity, int SharedCategories)>> FindSimilarUsersToUserAsync(
        Guid userId,
        int topN = 10,
        double minSimilarity = 0.0,
        CancellationToken cancellationToken = default)
    {
        var queryUser = await GetByUserIdAsync(userId, cancellationToken);
        if (queryUser?.Embedding == null)
            return new List<(Guid, double, int)>();

        var results = await FindSimilarUsersAsync(
            queryUser.Embedding,
            topN + 1,
            minSimilarity,
            cancellationToken);

        return results
            .Where(r => r.UserId != userId)
            .Take(topN)
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────
    // Bulk Queries
    // ─────────────────────────────────────────────────────────────

    public async Task<List<UserEmbedding>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any())
            return new List<UserEmbedding>();

        return await _dbSet
            .AsNoTracking()
            .Where(ue => idList.Contains(ue.Id) && ue.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserEmbedding>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ue => ue.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserEmbedding>> GetStaleEmbeddingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ue => ue.IsActive && ue.IsStale)
            .ToListAsync(cancellationToken);
    }



    public async Task<List<UserEmbedding>> GetNotCalculatedSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ue => ue.IsActive && ue.LastCalculated < since)
            .ToListAsync(cancellationToken);
    }


    public async Task<UserEmbedding> UpsertAsync(
        UserEmbedding embedding,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByUserIdAsync(embedding.UserId, cancellationToken);

        if (existing != null)
        {
            existing.UpdateFrom(embedding);
            _dbSet.Update(existing);
            return existing;
        }
        else
        {
            await _dbSet.AddAsync(embedding, cancellationToken);
            return embedding;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Analytics
    // ─────────────────────────────────────────────────────────────

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(ue => ue.IsActive, cancellationToken);
    }

    public async Task<int> GetLowConfidenceCountAsync(
        double threshold = 0.5,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(ue => ue.IsActive && ue.Confidence < threshold, cancellationToken);
    }


    // ─────────────────────────────────────────────────────────────
    // Cleanup
    // ─────────────────────────────────────────────────────────────

    public async Task<List<UserEmbedding>> GetArchivableAsync(
        int daysThreshold = 180,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

        return await _dbSet
            .AsNoTracking()
            .Where(ue => ue.IsActive &&
                        ue.IsStale &&
                        ue.LastCalculated < cutoff)
            .ToListAsync(cancellationToken);
    }
}