namespace AI.Application.Features.Recommendations.DTOs
{
    public class RecommendationResultDto
    {
        // The category name (e.g., "Jazz", "Technology")
        public string Category { get; set; } = default!;

        // The calculated strength (0.0 to Infinity)
        public double Score { get; set; }

        // Why we recommended it ("Based on your history" vs "Popular right now")
        public string Explanation { get; set; } = default!;
    }


    public sealed class RecommendationResultLiteDto
    {
        public Guid EventId { get; init; }

        public string Title { get; init; } = default!;

        public string? BannerUrl { get; init; }

        public DateTime? EventStartAt { get; init; }

        public decimal? MinPrice { get; init; }
    }

    public sealed class GeminiRecommendationResponse
    {
        public List<int> RankedIndexes { get; init; } = new();
    }
}