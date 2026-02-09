using AI.Domain.Repositories;
using AI.Application.Features.Recommendations.DTOs;
using AI.Application.Services;
using AI.Domain.Entities;

namespace AI.Infrastructure.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IUserInterestScoreRepository _userScoreRepo;
        private readonly IGlobalCategoryStatRepository _globalStatRepo;

        public RecommendationService(
            IUserInterestScoreRepository userScoreRepo,
            IGlobalCategoryStatRepository globalStatRepo)
        {
            _userScoreRepo = userScoreRepo;
            _globalStatRepo = globalStatRepo;
        }

        public async Task<List<RecommendationResultDto>> GetRecommendationsAsync(Guid? userId, int topN = 10)
        {
            // ---------------------------------------------------------
            // 1. Fetch Data
            // ---------------------------------------------------------
            // Fetch 2x the amount needed so we have enough candidates for the "Zipper" logic
            var globalStats = await _globalStatRepo.GetTopCategoriesAsync(topN * 2);

            var userScores = userId.HasValue
                ? await _userScoreRepo.GetAllForUserAsync(userId.Value)
                : new List<UserInterestScore>();

            // ---------------------------------------------------------
            // 2. Prepare Global Candidates (Global Pool)
            // ---------------------------------------------------------
            // We assume Global PopularityScore is 0-100. We normalize to 0.0 - 1.0.
            var globalCandidates = globalStats.Select(g => new RecommendationResultDto
            {
                Category = g.Category,
                Score = g.PopularityScore / 100.0, 
                Explanation = "Popular on platform"
            }).ToList();

            // ---------------------------------------------------------
            // 3. Prepare & NORMALIZE Personal Candidates (Personal Pool)
            // ---------------------------------------------------------
            var personalCandidates = new List<RecommendationResultDto>();

            if (userId.HasValue && userScores.Any())
            {
                // STEP A: Find the "Elephant" (The highest score the user has)
                // If your top score is 289.0, we divide everyone by 289.0.
                double maxUserScore = userScores.Max(u => u.Score);
                
                // Safety check to avoid division by zero
                if (maxUserScore == 0) maxUserScore = 1;

                // STEP B: Normalize User Scores to 0.0 - 1.0 range
                personalCandidates = userScores.Select(u => new RecommendationResultDto
                {
                    Category = u.Category,
                    Score = u.Score / maxUserScore, 
                    Explanation = $"Because you viewed {u.Category}"
                })
                .OrderByDescending(x => x.Score)
                .ToList();
            }

            // ---------------------------------------------------------
            // 4. THE ZIPPER MERGE (The "Mixup" Logic)
            // ---------------------------------------------------------
            // We alternate picking items: 1 Personal, 1 Global, 1 Personal, 1 Global...
            // This guarantees diversity even if personal scores are huge.
            
            var finalRecommendations = new List<RecommendationResultDto>();
            var usedCategories = new HashSet<string>();

            int pIndex = 0; // Personal Index
            int gIndex = 0; // Global Index

            while (finalRecommendations.Count < topN)
            {
                bool added = false;

                // --- SLOT A: Try to add a Personal Item (Odd slots) ---
                if (pIndex < personalCandidates.Count)
                {
                    var item = personalCandidates[pIndex++];
                    if (usedCategories.Add(item.Category)) // Returns false if already exists
                    {
                        finalRecommendations.Add(item);
                        added = true;
                    }
                }

                // Check if we are full before adding the next one
                if (finalRecommendations.Count >= topN) break;

                // --- SLOT B: Try to add a Global Item (Even slots) ---
                if (gIndex < globalCandidates.Count)
                {
                    var item = globalCandidates[gIndex++];
                    if (usedCategories.Add(item.Category))
                    {
                        finalRecommendations.Add(item);
                        added = true;
                    }
                }

                // --- EXIT CONDITION ---
                // If we couldn't add from either list (both are empty/exhausted), stop.
                if (!added && pIndex >= personalCandidates.Count && gIndex >= globalCandidates.Count)
                    break;
            }

            return finalRecommendations;
        }
    }
}