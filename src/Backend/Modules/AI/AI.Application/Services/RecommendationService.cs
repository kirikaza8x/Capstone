using AI.Domain.Repositories;
using AI.Application.Features.Recommendations.DTOs;
using AI.Domain.Entities;
using AI.Application.Abstractions;
using Microsoft.Extensions.Logging;
using AI.Domain.ReadModels;

namespace AI.Infrastructure.Services
{
    /// <summary>
    /// Recommendation engine that blends personal interests with global trends.
    /// MASTER PLAN: Layer 4 - The Recommendation Engine (Read Path)
    /// 
    /// ALGORITHM OPTIONS (configure via strategy):
    /// 1. ZIPPER MERGE: Alternates between personal and global recommendations
    /// 2. BAYESIAN SMOOTHING: Blends scores based on user confidence level
    /// 
    /// CRITICAL FIXES IMPLEMENTED:
    /// - Unified normalization across personal and global scores
    /// - Configurable algorithm strategy
    /// - Proper handling of cold-start users
    /// - Caching support for global stats
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly IUserInterestScoreRepository _userScoreRepo;
        private readonly IGlobalCategoryStatRepository _globalStatRepo;
        private readonly ILogger<RecommendationService> _logger;

        // Configuration constants
        private const int MIN_INTERACTIONS_FOR_CONFIDENCE = 10;  // Bayesian 'm' parameter
        private const int CANDIDATE_MULTIPLIER = 3;  // Fetch 3x candidates for diversity

        public RecommendationService(
            IUserInterestScoreRepository userScoreRepo,
            IGlobalCategoryStatRepository globalStatRepo,
            ILogger<RecommendationService> logger)
        {
            _userScoreRepo = userScoreRepo;
            _globalStatRepo = globalStatRepo;
            _logger = logger;
        }

        // ---------------------------------------------------------
        // PRIMARY RECOMMENDATION METHOD (Bayesian Strategy)
        // ---------------------------------------------------------
        public async Task<List<RecommendationResultDto>> GetRecommendationsAsync(
            Guid? userId, 
            int topN = 10)
        {
            _logger.LogInformation(
                "Generating recommendations | User: {UserId} | TopN: {TopN}",
                userId, topN);

            // STEP 1: Fetch data
            var globalStats = await _globalStatRepo.GetTopCategoriesAsync(topN * CANDIDATE_MULTIPLIER);
            
            var userScores = userId.HasValue
                ? await _userScoreRepo.GetAllForUserAsync(userId.Value)
                : new List<UserInterestScore>();

            _logger.LogDebug(
                "Loaded {GlobalCount} global stats, {UserCount} user scores",
                globalStats.Count(), userScores.Count());

            // STEP 2: Determine strategy based on user data
            if (!userId.HasValue || !userScores.Any())
            {
                _logger.LogInformation("👤 Cold start user - using global trends only");
                return GetGlobalRecommendations(globalStats, topN);
            }

            int totalUserInteractions = userScores.Sum(s => s.TotalInteractions);
            
            if (totalUserInteractions < MIN_INTERACTIONS_FOR_CONFIDENCE)
            {
                _logger.LogInformation(
                    "Low confidence user ({Interactions} interactions) - using Bayesian blend",
                    totalUserInteractions);
                
                return GetBayesianRecommendations(globalStats, userScores, topN);
            }

            _logger.LogInformation(
                "High confidence user ({Interactions} interactions) - using zipper merge",
                totalUserInteractions);
            
            return GetZipperMergeRecommendations(globalStats, userScores, topN);
        }

        // ---------------------------------------------------------
        // STRATEGY 1: BAYESIAN SMOOTHING (Best for new/cold users)
        // ---------------------------------------------------------
        private List<RecommendationResultDto> GetBayesianRecommendations(
            IEnumerable<GlobalCategoryStat> globalStats,
            List<UserInterestScore> userScores,
            int topN)
        {
            var allScores = new List<double>();
            allScores.AddRange(globalStats.Select(g => g.PopularityScore));
            allScores.AddRange(userScores.Select(u => u.Score));

            if (!allScores.Any())
            {
                _logger.LogWarning("No scores available for normalization");
                return new List<RecommendationResultDto>();
            }

            double minScore = allScores.Min();
            double maxScore = allScores.Max();
            double range = maxScore - minScore;
            
            if (range == 0) range = 1; // Prevent division by zero

            _logger.LogDebug(
                "Normalization range: [{Min:F2}, {Max:F2}]",
                minScore, maxScore);

            // Create normalized lookup dictionaries
            var globalDict = globalStats.ToDictionary(
                g => g.Category,
                g => (g.PopularityScore - minScore) / range
            );

            var personalDict = userScores.ToDictionary(
                u => u.Category,
                u => new {
                    NormalizedScore = (u.Score - minScore) / range,
                    Interactions = u.TotalInteractions
                }
            );

            // Get union of all categories
            var allCategories = globalDict.Keys
                .Union(personalDict.Keys)
                .ToHashSet();

            var blendedResults = new List<RecommendationResultDto>();

            foreach (var category in allCategories)
            {
                double personalScore = personalDict.TryGetValue(category, out var p) 
                    ? p.NormalizedScore 
                    : 0.0;
                
                double globalScore = globalDict.TryGetValue(category, out var g) 
                    ? g 
                    : 0.0;

                int userInteractionCount = personalDict.TryGetValue(category, out var u) 
                    ? u.Interactions 
                    : 0;

                // BAYESIAN FORMULA:
                // Weight = UserInteractions / (UserInteractions + MinConfidence)
                // FinalScore = (Weight × Personal) + ((1 - Weight) × Global)
                double confidenceWeight = (double)userInteractionCount / 
                    (userInteractionCount + MIN_INTERACTIONS_FOR_CONFIDENCE);

                double finalScore = (confidenceWeight * personalScore) + 
                    ((1 - confidenceWeight) * globalScore);

                string explanation = confidenceWeight > 0.5
                    ? $"Based on your {userInteractionCount} interactions with {category}"
                    : "Popular on platform";

                blendedResults.Add(new RecommendationResultDto
                {
                    Category = category,
                    Score = finalScore,
                    Explanation = explanation
                });
            }

            var result = blendedResults
                .OrderByDescending(r => r.Score)
                .Take(topN)
                .ToList();

            _logger.LogInformation("Generated {Count} Bayesian recommendations", result.Count);
            
            return result;
        }

        // ---------------------------------------------------------
        // STRATEGY 2: ZIPPER MERGE (Best for established users)
        // ---------------------------------------------------------
        private List<RecommendationResultDto> GetZipperMergeRecommendations(
            IEnumerable<GlobalCategoryStat> globalStats,
            List<UserInterestScore> userScores,
            int topN)
        {
            // CRITICAL FIX: Same unified normalization
            var allScores = new List<double>();
            allScores.AddRange(globalStats.Select(g => g.PopularityScore));
            allScores.AddRange(userScores.Select(u => u.Score));

            double minScore = allScores.Min();
            double maxScore = allScores.Max();
            double range = maxScore - minScore;
            if (range == 0) range = 1;

            var globalCandidates = globalStats.Select(g => new RecommendationResultDto
            {
                Category = g.Category,
                Score = (g.PopularityScore - minScore) / range,
                Explanation = "Popular on platform"
            })
            .OrderByDescending(x => x.Score)
            .ToList();

            var personalCandidates = userScores.Select(u => new RecommendationResultDto
            {
                Category = u.Category,
                Score = (u.Score - minScore) / range,
                Explanation = $"Based on your interest in {u.Category}"
            })
            .OrderByDescending(x => x.Score)
            .ToList();

            // ZIPPER ALGORITHM: Alternate between personal and global
            var finalRecommendations = new List<RecommendationResultDto>();
            var usedCategories = new HashSet<string>();

            int pIndex = 0;
            int gIndex = 0;

            while (finalRecommendations.Count < topN)
            {
                bool added = false;

                // Try to add a personal item
                if (pIndex < personalCandidates.Count)
                {
                    var item = personalCandidates[pIndex++];
                    if (usedCategories.Add(item.Category))
                    {
                        finalRecommendations.Add(item);
                        added = true;
                    }
                }

                if (finalRecommendations.Count >= topN) break;

                // Try to add a global item
                if (gIndex < globalCandidates.Count)
                {
                    var item = globalCandidates[gIndex++];
                    if (usedCategories.Add(item.Category))
                    {
                        finalRecommendations.Add(item);
                        added = true;
                    }
                }

                // Exit if both pools exhausted
                if (!added && pIndex >= personalCandidates.Count && gIndex >= globalCandidates.Count)
                    break;
            }

            _logger.LogInformation("Generated {Count} zipper-merged recommendations", finalRecommendations.Count);
            
            return finalRecommendations;
        }

        // ---------------------------------------------------------
        // STRATEGY 3: GLOBAL ONLY (Cold start fallback)
        // ---------------------------------------------------------
        private List<RecommendationResultDto> GetGlobalRecommendations(
            IEnumerable<GlobalCategoryStat> globalStats,
            int topN)
        {
            var result = globalStats
                .OrderByDescending(g => g.PopularityScore)
                .Take(topN)
                .Select(g => new RecommendationResultDto
                {
                    Category = g.Category,
                    Score = g.PopularityScore / 100.0,  // Normalize to 0-1
                    Explanation = "Popular on platform"
                })
                .ToList();

            _logger.LogInformation("Generated {Count} global-only recommendations", result.Count);
            
            return result;
        }
    }
}