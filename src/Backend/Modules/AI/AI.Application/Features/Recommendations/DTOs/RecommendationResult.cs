namespace AI.Application.Features.Recommendations.DTOs
{
    public class RecommendationResult
    {
        // The category name (e.g., "Jazz", "Technology")
        public string Category { get; set; } = default!;

        // The calculated strength (0.0 to Infinity)
        public double Score { get; set; }

        // Why we recommended it ("Based on your history" vs "Popular right now")
        public string Explanation { get; set; } = default!;
    }
}