using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Repositories;

/// <summary>
/// Repository for UserBehaviorLog.
/// Primary use: append-only logging + background job queries for enrichment.
/// 
/// STARTING SIMPLE: Basic EF Core queries first.
/// Advanced JSONB queries can be added later.
/// </summary>
public class UserBehaviorLogRepository : RepositoryBase<UserBehaviorLog, Guid>, IUserBehaviorLogRepository
{
    private readonly AIModuleDbContext _dbContext;
    private readonly DbSet<UserBehaviorLog> _dbSet;

    public UserBehaviorLogRepository(AIModuleDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<UserBehaviorLog>();
    }

    /// <summary>
    /// Fetches all raw logs for a user since a specific time.
    /// Used by enrichment pipeline to rebuild user embeddings.
    /// </summary>
    public async Task<List<UserBehaviorLog>> GetByUserIdSinceAsync(
        Guid userId,
        DateTime since,
        string? targetType = null,
        int limit = 200,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.UserId == userId && ubl.OccurredAt >= since);

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(ubl => ubl.TargetType == targetType);
        }

        return await query
            .OrderByDescending(ubl => ubl.OccurredAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets distinct categories a user has interacted with.
    /// 
    /// ⚠️ SIMPLE VERSION: Loads logs into memory, then parses categories in C#.
    /// This is fine for small datasets. For large-scale, we can optimize later
    /// with PostgreSQL JSONB functions.
    /// </summary>
    public async Task<List<string>> GetDistinctCategoriesForUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        // Step 1: Fetch logs for this user (targeting events)
        var logs = await _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.UserId == userId && ubl.TargetType == "event")
            .ToListAsync(ct);

        // Step 2: Extract categories in C# memory (simple, works immediately)
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var log in logs)
        {
            // Try "categories" key first (plural)
            if (log.Metadata.TryGetValue("categories", out var catValue) && !string.IsNullOrWhiteSpace(catValue))
            {
                // Parse: supports comma, semicolon, pipe delimiters
                var cats = catValue.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToLowerInvariant())
                    .Where(c => !string.IsNullOrWhiteSpace(c));

                foreach (var c in cats)
                    categories.Add(c);
            }
            // Fallback to "category" key (singular)
            else if (log.Metadata.TryGetValue("category", out var singleCat) && !string.IsNullOrWhiteSpace(singleCat))
            {
                categories.Add(singleCat.Trim().ToLowerInvariant());
            }
        }

        return categories.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// Gets the most recent interaction timestamp for a user.
    /// </summary>
    public async Task<DateTime?> GetLatestOccurrenceForUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.UserId == userId)
            .MaxAsync(ubl => (DateTime?)ubl.OccurredAt, ct);
    }

    /// <summary>
    /// Counts interactions by action type for a user.
    /// </summary>
    public async Task<Dictionary<string, int>> GetActionCountsForUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.UserId == userId)
            .GroupBy(ubl => ubl.ActionType)
            .Select(g => new { ActionType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ActionType, x => x.Count, ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Event-Centric Queries (SIMPLE VERSION)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all logs targeting a specific event.
    /// Used to measure event popularity / engagement signals.
    /// </summary>
    public async Task<List<UserBehaviorLog>> GetLogsForEventAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        var eventIdString = eventId.ToString();

        return await _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.TargetId == eventIdString && ubl.TargetType == "event")
            .OrderByDescending(ubl => ubl.OccurredAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets logs grouped by target event for batch processing.
    /// </summary>
    public async Task<Dictionary<Guid, List<UserBehaviorLog>>> GetLogsGroupedByEventAsync(
        IEnumerable<Guid> eventIds,
        CancellationToken ct = default)
    {
        var eventIdList = eventIds.ToList();
        if (!eventIdList.Any())
            return new Dictionary<Guid, List<UserBehaviorLog>>();

        // Convert Guids to strings for comparison with TargetId (stored as string)
        var eventIdStrings = eventIdList.Select(id => id.ToString()).ToHashSet();

        var logs = await _dbSet
            .AsNoTracking()
            .Where(ubl => eventIdStrings.Contains(ubl.TargetId) && ubl.TargetType == "event")
            .ToListAsync(ct);

        // Group by parsed Guid
        return logs
            .GroupBy(ubl => Guid.Parse(ubl.TargetId))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    // ─────────────────────────────────────────────────────────────
    // Background Job Queries (SIMPLE VERSION)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches all raw logs since a specific time (e.g., Last 24 Hours).
    /// We need raw logs because "category" is inside Metadata JSON,
    /// so grouping must happen in C# memory, not SQL.
    /// </summary>
    public async Task<List<UserBehaviorLog>> GetLogsSinceAsync(
        DateTime since,
        string? targetType = null,
        int limit = 1000,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.OccurredAt >= since);

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(ubl => ubl.TargetType == targetType);
        }

        return await query
            .OrderBy(ubl => ubl.OccurredAt)  // Oldest first for batch processing
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets logs that haven't been processed by enrichment yet.
    /// 
    /// ⚠️ SIMPLE VERSION: Returns recent logs as fallback.
    /// To properly track "processed" status, add a ProcessedAt column later:
    /// - Add: public DateTime? ProcessedAt { get; private set; } to entity
    /// - Add migration
    /// - Then filter: .Where(ubl => ubl.ProcessedAt == null)
    /// </summary>
    public async Task<List<UserBehaviorLog>> GetUnprocessedLogsAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        // Fallback: Return recent logs (likely unprocessed)
        var cutoff = DateTime.UtcNow.AddHours(-24);

        return await _dbSet
            .AsNoTracking()
            .Where(ubl => ubl.OccurredAt >= cutoff)
            .OrderBy(ubl => ubl.OccurredAt)  // Oldest first
            .Take(batchSize)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Analytics (SIMPLE VERSION)
    // ─────────────────────────────────────────────────────────────

    public async Task<long> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _dbSet.LongCountAsync(ct);
    }

    public async Task<long> GetCountByActionTypeAsync(string actionType, CancellationToken ct = default)
    {
        var normalized = actionType.ToLowerInvariant().Trim();
        return await _dbSet.LongCountAsync(ubl => ubl.ActionType == normalized, ct);
    }

    public async Task<long> GetCountByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.LongCountAsync(ubl => ubl.UserId == userId, ct);
    }
}
