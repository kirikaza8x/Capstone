// using Microsoft.EntityFrameworkCore;
// using Pgvector;
// using Shared.Infrastructure.Data;
// using AI.Domain.Entities;
// using AI.Domain.Repositories;
// using AI.Infrastructure.Data;
// using Pgvector.EntityFrameworkCore;

// namespace AI.Infrastructure.Repositories;

// public class EventEmbeddingRepository : RepositoryBase<EventEmbedding, Guid>, IEventEmbeddingRepository
// {
//     private readonly AIModuleDbContext _dbContext;
//     private readonly DbSet<EventEmbedding> _dbSet;

//     public EventEmbeddingRepository(AIModuleDbContext dbContext) : base(dbContext)
//     {
//         _dbContext = dbContext;
//         _dbSet = dbContext.Set<EventEmbedding>();
//     }

//     // ─────────────────────────────────────────────────────────────
//     // Basic Lookups
//     // ─────────────────────────────────────────────────────────────

//     public async Task<EventEmbedding?> GetByEventIdAsync(
//         Guid eventId, 
//         CancellationToken ct = default)
//     {
//         return await _dbSet
//             .AsNoTracking()
//             .FirstOrDefaultAsync(ee => ee.EventId == eventId, ct);
//     }

//     public async Task<bool> ExistsForEventAndModelAsync(
//         Guid eventId, 
//         string modelName, 
//         CancellationToken ct = default)
//     {
//         return await _dbSet
//             .AnyAsync(ee => ee.EventId == eventId && ee.ModelName == modelName, ct);
//     }

//     // ─────────────────────────────────────────────────────────────
//     // Batch Operations
//     // ─────────────────────────────────────────────────────────────

//     public async Task<List<Guid>> GetUnembeddedEventIdsAsync(
//         string modelName,
//         int batchSize, 
//         CancellationToken ct = default)
//     {
//         return await _dbContext.EventSnapshots
//             .Where(es => es.IsActive)
//             .Where(es => !_dbSet.Any(ee => 
//                 ee.EventId == es.Id && ee.ModelName == modelName))
//             .Select(es => es.Id)
//             .Take(batchSize)
//             .ToListAsync(ct);
//     }

//     public async Task<List<EventEmbedding>> GetStaleEmbeddingsAsync(
//         string modelName,
//         DateTime contentChangedAfter,
//         CancellationToken ct = default)
//     {
//         return await _dbSet
//             .Join(_dbContext.EventSnapshots,
//                 ee => ee.EventId,
//                 es => es.Id,
//                 (ee, es) => new { ee, es.SnapshotUpdatedAt })
//             .Where(x => x.SnapshotUpdatedAt > x.ee.EmbeddedAt)
//             .Select(x => x.ee)
//             .ToListAsync(ct);
//     }

//     // ─────────────────────────────────────────────────────────────
//     // ⭐ Vector Similarity Search (pgvector) — FIXED
//     // ─────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Cosine similarity search — returns top K event IDs closest to the query vector.
//     /// Uses pgvector HNSW index for fast approximate search.
//     /// Distance: lower = more similar (cosine distance: 0 = identical, 2 = opposite)
//     /// </summary>
//     public async Task<List<(Guid EventId, double Distance, string ModelName)>> SearchSimilarAsync(
//         float[] queryEmbedding,
//         int topK = 20,
//         float? maxDistance = null,
//         CancellationToken ct = default)
//     {
//         if (queryEmbedding == null || queryEmbedding.Length == 0)
//             throw new ArgumentException("Query embedding cannot be empty", nameof(queryEmbedding));

//         var queryVector = new Vector(queryEmbedding);

//         var query = _dbSet
//             .AsNoTracking()
//             .Select(ee => new
//             {
//                 ee.EventId,
//                 Distance = new Vector(ee.Embedding).CosineDistance(queryVector),
//                 ee.ModelName
//             });

//         // Apply distance filter BEFORE ordering
//         if (maxDistance.HasValue)
//         {
//             query = query.Where(x => x.Distance <= maxDistance.Value);
//         }

//         // ✅ Order at the END, then fetch, then convert to tuple
//         var results = await query
//             .OrderBy(x => x.Distance)
//             .Take(topK)
//             .ToListAsync(ct);

//         // ✅ Convert anonymous type to tuple AFTER fetching (in memory)
//         return results
//             .Select(x => (x.EventId, x.Distance, x.ModelName))
//             .ToList();
//     }

//     /// <summary>
//     /// Hybrid search: vector similarity + SQL filters (categories, active status, etc.)
//     /// </summary>
//     public async Task<List<(Guid EventId, double Distance, string ModelName)>> SearchSimilarWithFiltersAsync(
//         float[] queryEmbedding,
//         IEnumerable<string>? categories = null,
//         bool? isActive = null,
//         DateTime? minUpdatedAt = null,
//         int topK = 20,
//         float? maxDistance = null,
//         CancellationToken ct = default)
//     {
//         var queryVector = new Vector(queryEmbedding);

//         // ✅ Build base query with anonymous type (not tuple)
//         var query = _dbSet
//             .AsNoTracking()
//             .Join(_dbContext.EventSnapshots,
//                 ee => ee.EventId,
//                 es => es.Id,
//                 (ee, es) => new { ee, es })
//             .Select(x => new
//             {
//                 x.ee.EventId,
//                 Distance = new Vector(x.ee.Embedding).CosineDistance(queryVector),
//                 x.ee.ModelName,
//                 x.es.Categories,
//                 x.es.IsActive,
//                 x.es.SnapshotUpdatedAt
//             });

//         // ✅ Apply ALL filters BEFORE ordering
//         if (isActive.HasValue)
//         {
//             query = query.Where(x => x.IsActive == isActive.Value);
//         }

//         if (minUpdatedAt.HasValue)
//         {
//             query = query.Where(x => x.SnapshotUpdatedAt >= minUpdatedAt.Value);
//         }

//         if (categories?.Any() == true)
//         {
//             var categoryList = categories.ToList();
//             // ⚠️ JsonExists on JSONB collection - may need in-memory fallback
//             query = query.Where(x => categoryList.Any(c => 
//                 EF.Functions.JsonExists(x.Categories, c)));
//         }

//         if (maxDistance.HasValue)
//         {
//             query = query.Where(x => x.Distance <= maxDistance.Value);
//         }

//         // ✅ Order at the END, then fetch, then convert to tuple
//         var results = await query
//             .OrderBy(x => x.Distance)
//             .Take(topK)
//             .ToListAsync(ct);

//         // ✅ Convert anonymous type to tuple AFTER fetching (in memory)
//         return results
//             .Select(x => (x.EventId, x.Distance, x.ModelName))
//             .ToList();
//     }

//     // ─────────────────────────────────────────────────────────────
//     // Analytics
//     // ─────────────────────────────────────────────────────────────

//     public async Task<int> GetCountByModelAsync(string modelName, CancellationToken ct = default)
//     {
//         return await _dbSet.CountAsync(ee => ee.ModelName == modelName, ct);
//     }

//     public async Task<DateTime?> GetLatestEmbeddedAtAsync(string modelName, CancellationToken ct = default)
//     {
//         return await _dbSet
//             .Where(ee => ee.ModelName == modelName)
//             .MaxAsync(ee => (DateTime?)ee.EmbeddedAt, ct);
//     }
// }