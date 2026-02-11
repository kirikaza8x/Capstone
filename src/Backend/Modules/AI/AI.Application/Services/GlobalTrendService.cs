using AI.Application.Abstractions;
using AI.Domain.ReadModels;
using AI.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AI.Application.Services;

public class GlobalTrendService : IGlobalTrendService
{
    private readonly IUserBehaviorLogRepository _logRepo;
    private readonly IGlobalCategoryStatRepository _globalRepo;
    private readonly ILogger<GlobalTrendService> _logger;

    public GlobalTrendService(
        IUserBehaviorLogRepository logRepo, 
        IGlobalCategoryStatRepository globalRepo,
        ILogger<GlobalTrendService> logger)
    {
        _logRepo = logRepo;
        _globalRepo = globalRepo;
        _logger = logger;
    }

    public async Task UpdateGlobalTrendsAsync()
    {
        _logger.LogInformation("Starting Global Trend Calculation...");

        // 1. Time Window: Last 24 Hours
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var recentLogs = await _logRepo.GetLogsSinceAsync(cutoff);
        
        if (!recentLogs.Any())
        {
            _logger.LogInformation("No user activity found in the last 24h.");
            return;
        }

        // 2. Aggregate Data In-Memory
        var categoryAggregates = new Dictionary<string, (double WeightedScore, int RawCount)>();

        foreach (var log in recentLogs)
        {
            if (log.Metadata != null && 
                log.Metadata.TryGetValue("category", out string? rawCategory) && 
                !string.IsNullOrWhiteSpace(rawCategory))
            {
                string key = rawCategory.Trim().ToLowerInvariant(); // Normalize Key

                if (!categoryAggregates.ContainsKey(key))
                    categoryAggregates[key] = (0.0, 0);

                // WEIGHTING LOGIC: Purchase (5x) vs View (1x)
                double weight = log.ActionType.Equals("purchase", StringComparison.OrdinalIgnoreCase) ? 5.0 : 1.0;
                
                var current = categoryAggregates[key];
                categoryAggregates[key] = (current.WeightedScore + weight, current.RawCount + 1);
            }
        }

        if (!categoryAggregates.Any()) return;

        // 3. Normalize Scores
        double maxScore = categoryAggregates.Values.Max(x => x.WeightedScore);

        // 4. UPSERT LOGIC (The Fix) --------------------------------------
        
        // A. Get list of categories we calculated
        var calculatedCategories = categoryAggregates.Keys.ToList();

        // B. Fetch existing DB entries for these categories
        // (You need to ensure your Repo has this method, see below)
        var existingStats = await _globalRepo.GetByCategoryNamesAsync(calculatedCategories);

        var statsToUpdate = new List<GlobalCategoryStat>();
        var statsToAdd = new List<GlobalCategoryStat>();

        foreach (var kvp in categoryAggregates)
        {
            string catName = kvp.Key;
            double normalizedScore = (kvp.Value.WeightedScore / maxScore) * 100.0;
            int count = kvp.Value.RawCount;

            // Check if exists in DB
            var existingEntity = existingStats.FirstOrDefault(e => e.Category == catName);

            if (existingEntity != null)
            {
                // UPDATE existing
                existingEntity.UpdateStats(normalizedScore, count); // Ensure method exists on Entity
                statsToUpdate.Add(existingEntity);
            }
            else
            {
                // INSERT new
                var newStat = GlobalCategoryStat.Create(catName, normalizedScore, count);
                statsToAdd.Add(newStat);
            }
        }

        // C. Save Changes
        if (statsToUpdate.Any())  _globalRepo.UpdateRange(statsToUpdate);
        if (statsToAdd.Any())  _globalRepo.AddRange(statsToAdd);
        // ----------------------------------------------------------------

        _logger.LogInformation("Global Trends Updated. {Updated} updated, {Added} added.", statsToUpdate.Count, statsToAdd.Count);
    }
}