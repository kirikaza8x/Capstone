using AI.Domain.Repositories;
using AI.Application.Features.Recommendations.DTOs;
using AI.Application.Services;

namespace AI.Infrastructure.Services
{
    

    public class RecommendationService : IRecommendationService
    {
        private readonly IUserInterestScoreRepository _userScoreRepo;
        private readonly IGlobalCategoryStatRepository _globalStatRepo;

        // TUNABLE CONSTANT: "The Confidence Threshold"
        // How many interactions does a user need before we trust their taste over the global average?
        // 5.0 means: "At 5 interactions, we trust the user 50% and the crowd 50%."
        private const double ConfidenceConstant = 5.0;

        public RecommendationService(
            IUserInterestScoreRepository userScoreRepo,
            IGlobalCategoryStatRepository globalStatRepo)
        {
            _userScoreRepo = userScoreRepo;
            _globalStatRepo = globalStatRepo;
        }

        public async Task<List<RecommendationResultDto>> GetRecommendationsAsync(Guid? userId, int topN = 10)
        {
            // 1. Fetch Global Stats (The Baseline)
            // In a real app, cache this for 10-60 minutes!
            var globalStats = await _globalStatRepo.GetTopCategoriesAsync(topN);

            // Normalize Global Scores to 0-1 range for fair math
            // (Assumes PopularityScore is 0-100, so we divide by 100)
            // If your Global Stats are already normalized, skip this.
            var globalMap = globalStats.ToDictionary(
                k => k.Category,
                v => v.PopularityScore / 100.0
            );

            // 2. Fetch User Personal Scores (if logged in)
            var userScores = userId.HasValue
                ? await _userScoreRepo.GetAllForUserAsync(userId.Value)
                : new List<AI.Domain.Entities.UserInterestScore>();

            // 3. The Great Unification (Bayesian Smoothing)
            var allCategories = globalMap.Keys.Union(userScores.Select(u => u.Category)).Distinct();
            var recommendations = new List<RecommendationResultDto>();

            foreach (var category in allCategories)
            {
                // A. Get Global Component
                double globalScore = globalMap.GetValueOrDefault(category, 0.0);

                // B. Get Personal Component
                var userInterest = userScores.FirstOrDefault(u => u.Category == category);
                double personalScore = userInterest?.Score ?? 0.0;

                // USE THE  COUNT
                int interactionCount = userInterest?.TotalInteractions ?? 0;

                // C. Calculate Confidence Factor (Bayesian Logic)
                // 0 interactions -> Factor 0.0 (Pure Global)
                // 5 interactions -> Factor 0.5 (Mixed)
                // 50 interactions -> Factor 0.9 (Pure Personal)
                double confidenceFactor = interactionCount / (ConfidenceConstant + interactionCount);

                // D. The Formula
                double finalScore = (personalScore * confidenceFactor) + (globalScore * (1.0 - confidenceFactor));

                recommendations.Add(new RecommendationResultDto
                {
                    Category = category,
                    Score = finalScore,
                    Explanation = confidenceFactor > 0.5 ? "Based on your interests" : "Popular on platform"
                });
            }

            // 4. Sort and Return
            return recommendations
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .ToList();
        }
    }
}