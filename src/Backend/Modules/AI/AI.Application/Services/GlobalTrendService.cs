using AI.Application.Abstractions;
using AI.Domain.ReadModels;
using AI.Domain.Repositories;
using AI.Domain.Interfaces.UOW;
using Microsoft.Extensions.Logging;

namespace AI.Application.Services
{
    /// <summary>
    /// Background service for calculating global category trends.
    /// MASTER PLAN: Layer 3 - Global Intelligence
    ///
    /// WORKFLOW:
    /// 1. Fetch recent activity logs (last 24 hours) — IN PARALLEL with weights + existing stats
    /// 2. Apply DECAY to ALL existing categories (not just inactive ones)
    /// 3. Aggregate new activity with dynamic weights
    /// 4. Normalize scores using logarithmic scaling
    /// 5. UPSERT into GlobalCategoryStat table
    /// 6. Cleanup near-zero dead categories
    ///
    /// OPTIMIZATIONS:
    /// - True parallel data fetching via Task.WhenAll
    /// - Logarithmic normalization to compress popularity gap
    /// - Exponential decay ensures trends fade naturally over time
    /// </summary>
    public class GlobalTrendService : IGlobalTrendService
    {
        private readonly IUserBehaviorLogRepository _logRepo;
        private readonly IGlobalCategoryStatRepository _globalRepo;
        private readonly IInteractionWeightRepository _weightRepo;
        private readonly IAiUnitOfWork _unitOfWork;
        private readonly ILogger<GlobalTrendService> _logger;

        // Configuration constants
        private const int LOOKBACK_HOURS = 24;
        private const double DAILY_DECAY_FACTOR = 0.95;  // 5% decay per run
        private const double MIN_SCORE_THRESHOLD = 0.1;  // Floor scores below this to zero

        public GlobalTrendService(
            IUserBehaviorLogRepository logRepo,
            IGlobalCategoryStatRepository globalRepo,
            IInteractionWeightRepository weightRepo,
            IAiUnitOfWork unitOfWork,
            ILogger<GlobalTrendService> logger)
        {
            _logRepo = logRepo;
            _globalRepo = globalRepo;
            _weightRepo = weightRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task UpdateGlobalTrendsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting Global Trend Calculation...");

            try
            {
                // ---------------------------------------------------------
                // STEP 1: FETCH DATA IN PARALLEL (Real parallel)
                // Previously: each repo call was awaited one by one (3 sequential DB round-trips)
                // Now: all 3 fire simultaneously, total wait = slowest query, not sum of all
                // ---------------------------------------------------------
                var cutoff = DateTime.UtcNow.AddHours(-LOOKBACK_HOURS);

                var recentLogsTask = _logRepo.GetLogsSinceAsync(cutoff);
                var weightsTask = _weightRepo.FindAsync(w => w.IsActive);
                var allExistingStatsTask = _globalRepo.GetAllAsync();

                await Task.WhenAll(recentLogsTask, weightsTask, allExistingStatsTask);

                // Unwrap results — all are already completed at this point, no extra await cost
                var recentLogs = await recentLogsTask;
                var weights = (await weightsTask)
                    .ToDictionary(
                        w => w.ActionType.ToLower().Trim(),
                        w => w.Weight
                    );
                var allExistingStats = (await allExistingStatsTask).ToList();  // Materialize once

                _logger.LogInformation(
                    "Loaded {LogCount} logs, {WeightCount} weights, {StatCount} existing stats",
                    recentLogs.Count(), weights.Count, allExistingStats.Count);

                // ---------------------------------------------------------
                // STEP 2: APPLY DECAY TO ALL EXISTING CATEGORIES
                // Uses GlobalCategoryStat.ApplyDecay(factor) — flat multiplier model.
                // Note: UserInterestScore uses a different half-life model. These are intentionally
                // separate: global trends decay on a fixed schedule, personal scores decay by time elapsed.
                // ---------------------------------------------------------
                _logger.LogInformation("Applying decay to all {Count} existing categories...", allExistingStats.Count);

                foreach (var stat in allExistingStats)
                {
                    stat.ApplyDecay(DAILY_DECAY_FACTOR);
                    _globalRepo.Update(stat);
                }

                // ---------------------------------------------------------
                // STEP 3: AGGREGATE NEW ACTIVITY FROM LOGS
                // ---------------------------------------------------------
                var categoryAggregates = new Dictionary<string, (double WeightedScore, int RawCount)>();

                foreach (var log in recentLogs)
                {
                    var categories = log.GetCategories();

                    if (categories == null || !categories.Any())
                        continue;

                    // Look up weight from DB; default to 1.0 if action not defined
                    string actionKey = log.ActionType.ToLower().Trim();
                    double weight = weights.TryGetValue(actionKey, out double w) ? w : 1.0;

                    foreach (var category in categories)
                    {
                        string key = category.Trim().ToLowerInvariant();

                        if (!categoryAggregates.ContainsKey(key))
                            categoryAggregates[key] = (0.0, 0);

                        var current = categoryAggregates[key];
                        categoryAggregates[key] = (
                            current.WeightedScore + weight,
                            current.RawCount + 1
                        );
                    }
                }

                _logger.LogInformation(
                    "Aggregated {Count} categories from recent activity",
                    categoryAggregates.Count);

                // ---------------------------------------------------------
                // STEP 4: EARLY EXIT — no new activity, decay-only run
                // ---------------------------------------------------------
                if (!categoryAggregates.Any())
                {
                    _logger.LogInformation("No new activity detected. Stats have been decayed only.");
                    await _unitOfWork.SaveChangesAsync();
                    return;
                }

                // ---------------------------------------------------------
                // STEP 5: LOGARITHMIC NORMALIZATION
                // Purpose: Compresses the gap between mega-popular and niche categories.
                // Example: raw scores [1, 10, 1000] → log scores [0, 23, 100] instead of [0.1, 1, 100]
                // Alternative: Use Math.Sqrt() for gentler compression.
                // ---------------------------------------------------------
                double maxScore = categoryAggregates.Values.Max(x => x.WeightedScore);
                double logMax = Math.Log(maxScore + 1);

                _logger.LogDebug("Max weighted score: {Max}, Log(Max): {LogMax}", maxScore, logMax);

                // ---------------------------------------------------------
                // STEP 6: UPSERT STATS (UPDATE existing or CREATE new)
                // ---------------------------------------------------------
                var existingStatsDict = allExistingStats.ToDictionary(s => s.Category);
                int updated = 0;
                int created = 0;

                foreach (var kvp in categoryAggregates)
                {
                    string categoryName = kvp.Key;
                    double rawScore = kvp.Value.WeightedScore;
                    int count = kvp.Value.RawCount;

                    double logCurrent = Math.Log(rawScore + 1);
                    double normalizedScore = (logCurrent / logMax) * 100.0;

                    if (existingStatsDict.TryGetValue(categoryName, out var existingStat))
                    {
                        // Accumulate on top of already-decayed score
                        double newScore = existingStat.PopularityScore + normalizedScore;
                        int newCount = existingStat.TotalInteractions + count;

                        existingStat.UpdateStats(newScore, newCount, rawScore);
                        _globalRepo.Update(existingStat);
                        updated++;
                    }
                    else
                    {
                        var newStat = GlobalCategoryStat.Create(
                            categoryName,
                            normalizedScore,
                            count,
                            rawScore);

                        _globalRepo.Add(newStat);
                        created++;
                    }
                }

                // ---------------------------------------------------------
                // STEP 7: CLEANUP — remove truly dead categories
                // Condition: score AND interactions both floored to zero after decay.
                // These are safe to delete because they have no signal left.
                // Note: Categories with score=0 but TotalInteractions > 0 are kept
                // for analytics (they still have historical interaction data).
                // ---------------------------------------------------------
                var statsToCleanup = allExistingStats
                    .Where(s => s.PopularityScore < MIN_SCORE_THRESHOLD && s.TotalInteractions == 0)
                    .ToList();

                if (statsToCleanup.Any())
                {
                    _logger.LogInformation(
                        "Cleaning up {Count} fully dead categories (score < {Threshold}, interactions = 0)",
                        statsToCleanup.Count, MIN_SCORE_THRESHOLD);

                    _globalRepo.RemoveRange(statsToCleanup);
                }

                // ---------------------------------------------------------
                // STEP 8: SAVE ALL CHANGES (single transaction)
                // ---------------------------------------------------------
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Global Trends Updated | Created: {Created} | Updated: {Updated} | Decayed: {Decayed} | Cleaned: {Cleaned}",
                    created, updated, allExistingStats.Count, statsToCleanup.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL FAILURE in Global Trend Calculation");
                throw;
            }
        }
    }
}