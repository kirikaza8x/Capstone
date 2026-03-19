using AI.Domain.ReadModels;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that updates GlobalCategoryStat from UserInterestScore data.
///
/// RUNS: Every 6 hours (configurable).
///
/// ALGORITHM:
///   1. Load all active UserInterestScores grouped by category
///   2. For each category: sum scores across all users → raw weighted score
///   3. Apply decay to existing GlobalCategoryStats (old popularity fades)
///   4. Upsert with new scores
///   5. Normalise to 0-100 scale
///
/// WHY: GlobalCategoryStat powers cold-start recommendations.
/// Without it, new users get no recommendations.
/// </summary>
public sealed class GlobalCategoryStatUpdateJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GlobalCategoryStatUpdateJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);
    private const double DecayFactor = 0.95; // 5% decay per run = soft forgetting

    public GlobalCategoryStatUpdateJob(
        IServiceScopeFactory scopeFactory,
        ILogger<GlobalCategoryStatUpdateJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("GlobalCategoryStatUpdateJob started — interval: {Interval}h",
            _interval.TotalHours);

        // Wait a bit on startup so DB migrations finish first
        await Task.Delay(TimeSpan.FromSeconds(30), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "GlobalCategoryStatUpdateJob failed — will retry next interval");
            }

            await Task.Delay(_interval, ct);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AIModuleDbContext>();

        _logger.LogInformation("Running GlobalCategoryStat update...");

        // ── 1. Aggregate interest scores by category across all users ──
        var categoryAggregates = await dbContext.Set<AI.Domain.Entities.UserInterestScore>()
            .Where(s => s.IsActive && s.Score > 0)
            .GroupBy(s => s.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalScore = g.Sum(s => s.Score),
                TotalInteractions = g.Sum(s => s.TotalInteractions),
                UserCount = g.Count()
            })
            .ToListAsync(ct);

        if (categoryAggregates.Count == 0)
        {
            _logger.LogDebug("No interest scores found — skipping GlobalCategoryStat update");
            return;
        }

        // ── 2. Normalise to 0-100 scale ───────────────────────────
        var maxScore = categoryAggregates.Max(a => a.TotalScore);
        if (maxScore <= 0) return;

        // ── 3. Load existing stats ────────────────────────────────
        var existingStats = await dbContext.Set<GlobalCategoryStat>()
            .ToDictionaryAsync(s => s.Category, ct);

        var toAdd = new List<GlobalCategoryStat>();
        var toUpdate = new List<GlobalCategoryStat>();

        // ── 4. Apply decay to all existing stats first ────────────
        foreach (var stat in existingStats.Values)
        {
            stat.ApplyDecay(DecayFactor);
            toUpdate.Add(stat);
        }

        // ── 5. Upsert with new aggregated scores ──────────────────
        foreach (var agg in categoryAggregates)
        {
            var normalised = (agg.TotalScore / maxScore) * 100.0;
            var rawScore = agg.TotalScore;

            if (existingStats.TryGetValue(agg.Category, out var existing))
            {
                // AddActivity on top of decayed score — recency + history blended
                existing.AddActivity(normalised * 0.1, agg.TotalInteractions);
            }
            else
            {
                toAdd.Add(GlobalCategoryStat.Create(
                    category: agg.Category,
                    score: normalised,
                    count: agg.TotalInteractions,
                    rawScore: rawScore
                ));
            }
        }

        // ── 6. Persist ────────────────────────────────────────────
        if (toAdd.Count > 0)
            await dbContext.Set<GlobalCategoryStat>().AddRangeAsync(toAdd, ct);

        if (toUpdate.Count > 0)
            dbContext.Set<GlobalCategoryStat>().UpdateRange(toUpdate);

        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "GlobalCategoryStat updated — {Added} added, {Updated} updated, {Total} total categories",
            toAdd.Count, toUpdate.Count, categoryAggregates.Count);
    }
}