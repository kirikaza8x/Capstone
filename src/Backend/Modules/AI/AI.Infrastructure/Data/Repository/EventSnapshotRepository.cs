// using Microsoft.EntityFrameworkCore;
// using Shared.Infrastructure.Data;
// using AI.Domain.Entities;
// using AI.Domain.Repositories;
// using AI.Infrastructure.Data;

// namespace AI.Infrastructure.Repositories;

// public class EventSnapshotRepository : RepositoryBase<EventSnapshot, Guid>, IEventSnapshotRepository
// {
//     private readonly AIModuleDbContext _dbContext;
//     private readonly DbSet<EventSnapshot> _dbSet;

//     public EventSnapshotRepository(AIModuleDbContext dbContext) : base(dbContext)
//     {
//         _dbContext = dbContext;
//         _dbSet = dbContext.Set<EventSnapshot>();
//     }

//     public async Task<EventSnapshot?> GetByEventIdAsync(
//         Guid eventId, 
//         CancellationToken ct = default)
//     {
//         return await _dbSet
//             .AsNoTracking()
//             .FirstOrDefaultAsync(es => es.Id == eventId, ct);
//     }

//     public async Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default)
//     {
//         return await _dbSet.AnyAsync(es => es.Id == eventId, ct);
//     }

//     public async Task<List<EventSnapshot>> GetActiveByCategoriesAsync(
//         IEnumerable<string> categories,
//         int limit = 50,
//         CancellationToken ct = default)
//     {
//         var categoryList = categories.ToList();
//         if (!categoryList.Any())
//             return new List<EventSnapshot>();
//         return await _dbSet
//             .AsNoTracking()
//             .Where(es => es.IsActive)
//             .Where(es => categoryList.Any(c => 
//                 EF.Functions.JsonExists(es.Categories, c)))
//             .OrderByDescending(es => es.SnapshotUpdatedAt)
//             .Take(limit)
//             .ToListAsync(ct);
//     }

//     public async Task<List<EventSnapshot>> GetActiveByHashtagsAsync(
//         IEnumerable<string> hashtags,
//         int limit = 50,
//         CancellationToken ct = default)
//     {
//         var hashtagList = hashtags.ToList();
//         if (!hashtagList.Any())
//             return new List<EventSnapshot>();

//         return await _dbSet
//             .AsNoTracking()
//             .Where(es => es.IsActive)
//             .Where(es => hashtagList.Any(h => 
//                 EF.Functions.JsonExists(es.Hashtags, h)))
//             .OrderByDescending(es => es.SnapshotUpdatedAt)
//             .Take(limit)
//             .ToListAsync(ct);
//     }

//     public async Task<List<EventSnapshot>> SearchByTextAsync(
//         string query,
//         int limit = 50,
//         CancellationToken ct = default)
//     {
//         var searchTerm = query.ToLowerInvariant();

//         return await _dbSet
//             .AsNoTracking()
//             .Where(es => es.IsActive && 
//                 (es.Title.ToLower().Contains(searchTerm) || 
//                  es.Description.ToLower().Contains(searchTerm)))
//             .OrderByDescending(es => es.SnapshotUpdatedAt)
//             .Take(limit)
//             .ToListAsync(ct);
//     }

//     public async Task<List<Guid>> GetUnembeddedEventIdsAsync(
//         string embeddingModelName,
//         int batchSize, 
//         CancellationToken ct = default)
//     {
//         return await _dbSet
//             .Where(es => es.IsActive)
//             .Where(es => !_dbContext.EventEmbeddings.Any(ee => 
//                 ee.EventId == es.Id && ee.ModelName == embeddingModelName))
//             .Select(es => es.Id)
//             .Take(batchSize)
//             .ToListAsync(ct);
//     }

//     public async Task<List<EventSnapshot>> GetChangedSinceEmbeddingAsync(
//         string embeddingModelName,
//         int batchSize,
//         CancellationToken ct = default)
//     {
//         return await _dbSet
//             .Join(_dbContext.EventEmbeddings,
//                 es => es.Id,
//                 ee => ee.EventId,
//                 (es, ee) => new { es, ee.EmbeddedAt, ee.ModelName })
//             .Where(x => x.ModelName == embeddingModelName && 
//                        x.es.SnapshotUpdatedAt > x.EmbeddedAt)
//             .Select(x => x.es)
//             .Take(batchSize)
//             .ToListAsync(ct);
//     }

//     public async Task<List<EventSnapshot>> GetActiveAsync(
//         DateTime? minUpdatedAt = null,
//         int limit = 100,
//         CancellationToken ct = default)
//     {
//         var query = _dbSet
//             .AsNoTracking()
//             .Where(es => es.IsActive);

//         if (minUpdatedAt.HasValue)
//             query = query.Where(es => es.SnapshotUpdatedAt >= minUpdatedAt.Value);

//         return await query
//             .OrderByDescending(es => es.SnapshotUpdatedAt)
//             .Take(limit)
//             .ToListAsync(ct);
//     }

//     public async Task DeactivateAsync(Guid eventId, CancellationToken ct = default)
//     {
//         var snapshot = await _dbSet.FindAsync(new object[] { eventId }, ct);
//         if (snapshot != null)
//         {
//             snapshot.Deactivate();
//             _dbSet.Update(snapshot);
//             await _dbContext.SaveChangesAsync(ct);
//         }
//     }
// }
