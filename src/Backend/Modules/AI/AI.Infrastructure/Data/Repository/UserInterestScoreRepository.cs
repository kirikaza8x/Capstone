using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Repositories;

public class UserInterestScoreRepository : RepositoryBase<UserInterestScore, Guid>, IUserInterestScoreRepository
{
    private readonly AIModuleDbContext _dbContext;
    private readonly DbSet<UserInterestScore> _dbSet;

    public UserInterestScoreRepository(AIModuleDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<UserInterestScore>();
    }

    public async Task<UserInterestScore?> GetByUserAndCategoryAsync(
        Guid userId,
        string category,
        CancellationToken ct = default)
    {
        var normalized = category.ToLowerInvariant().Trim();
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Category == normalized &&
                x.IsActive,
            ct);
    }

    public async Task<List<UserInterestScore>> GetAllForUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.Score)
            .ToListAsync(ct);
    }

    public void Delete(UserInterestScore entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<List<UserInterestScore>> GetByUserAndCategoriesAsync(
        Guid userId,
        List<string> categories,
        CancellationToken ct = default)
    {
        if (!categories.Any())
            return new List<UserInterestScore>();

        var normalized = categories
            .Select(c => c.ToLowerInvariant().Trim())
            .ToList();

        return await _dbSet
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                normalized.Contains(x.Category) &&
                x.IsActive)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Upsert with decay for a single category.
    /// Does NOT call SaveChangesAsync — caller must commit via UoW.
    /// </summary>
    public async Task<UserInterestScore> UpsertWithDecayAsync(
        Guid userId,
        string category,
        double weight,
        double halfLifeDays,
        CancellationToken ct = default)
    {
        var normalizedCategory = category.ToLowerInvariant().Trim();
        int maxRetries = 3;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var tracked = _dbContext.ChangeTracker
                    .Entries<UserInterestScore>()
                    .FirstOrDefault(e =>
                        e.Entity.UserId   == userId &&
                        e.Entity.Category == normalizedCategory &&
                        e.Entity.IsActive)
                    ?.Entity;

                if (tracked is not null)
                {
                    tracked.DecayAndAdd(weight, halfLifeDays);
                    return tracked;
                }

                // Query with tracking so EF owns the instance
                var existing = await _dbSet
                    .FirstOrDefaultAsync(x =>
                        x.UserId   == userId &&
                        x.Category == normalizedCategory &&
                        x.IsActive, ct);

                if (existing is not null)
                {
                    existing.DecayAndAdd(weight, halfLifeDays);
                    return existing;
                }

                var newScore = UserInterestScore.Create(userId, normalizedCategory, weight);
                _dbSet.Add(newScore);
                return newScore;
            }
            catch (DbUpdateException ex) when (
                ex.InnerException?.Message.Contains("duplicate") == true ||
                ex.InnerException?.Message.Contains("unique")    == true)
            {
                _dbContext.ChangeTracker.Clear();

                if (attempt == maxRetries - 1)
                    throw;
            }
        }

        throw new InvalidOperationException(
            "Failed to upsert UserInterestScore after retries");
    }

    /// <summary>
    /// Bulk upsert for multiple categories.
    /// Does NOT call SaveChangesAsync — caller must commit via UoW.
    /// </summary>
    public async Task<List<UserInterestScore>> BulkUpsertWithDecayAsync(
        Guid userId,
        Dictionary<string, double> categoryWeights,
        double halfLifeDays,
        CancellationToken ct = default)
    {
        var results = new List<UserInterestScore>();

        foreach (var (category, weight) in categoryWeights)
        {
            var result = await UpsertWithDecayAsync(
                userId, category, weight, halfLifeDays, ct);
            results.Add(result);
        }

        return results;
    }

    public async Task<List<UserInterestScore>> GetTopCategoriesAsync(
        Guid userId,
        int topN = 10,
        double minScore = 1.0,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && x.Score >= minScore)
            .OrderByDescending(x => x.Score)
            .Take(topN)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetCategoriesAboveThresholdAsync(
        Guid userId,
        double minScore,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && x.Score >= minScore)
            .Select(x => x.Category)
            .ToListAsync(ct);
    }

    public async Task<List<UserInterestScore>> GetStaleScoresAsync(
        int daysThreshold = 90,
        double maxScore = 1.0,
        CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.LastUpdated < cutoff &&
                        x.Score < maxScore)
            .ToListAsync(ct);
    }

    public async Task<int> GetInterestCountAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .CountAsync(x => x.UserId == userId && x.IsActive, ct);
    }

    public async Task<Dictionary<string, object>> GetAggregateStatsAsync(
        CancellationToken ct = default)
    {
        var totalUsers      = await _dbSet.Select(x => x.UserId).Distinct().CountAsync(ct);
        var totalCategories = await _dbSet.Select(x => x.Category).Distinct().CountAsync(ct);
        var avgScore        = await _dbSet.AverageAsync(x => (double?)x.Score) ?? 0;

        return new Dictionary<string, object>
        {
            { "TotalUsers",      totalUsers },
            { "TotalCategories", totalCategories },
            { "AverageScore",    Math.Round(avgScore, 2) },
            { "Timestamp",       DateTime.UtcNow }
        };
    }

    public async Task<int> ArchiveStaleScoresAsync(
        int daysThreshold = 180,
        double maxScore = 0.5,
        CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);

        var stale = await _dbSet
            .Where(x => x.IsActive &&
                        x.LastUpdated < cutoff &&
                        x.Score < maxScore)
            .ToListAsync(ct);

        foreach (var score in stale)
            score.IsActive = false;

        // ArchiveStale is a batch admin operation — SaveChanges here is acceptable
        // since it's not part of a request pipeline UoW
        return await _dbContext.SaveChangesAsync(ct);
    }
}