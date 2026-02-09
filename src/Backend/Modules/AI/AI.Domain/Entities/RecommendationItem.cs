using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    // This can be an Entity or a Value Object depending on your preference.
    // Usually Entity is easier for EF Core relationships.
    public class RecommendationItem : Entity<Guid>
    {
        public Guid RecommendationSetId { get; private set; }
        
        public string Category { get; private set; } = default!; // Or TargetId/ProductId
        
        // The final Bayesian score used to rank this item
        public double Score { get; private set; }
        
        // 1 = Top pick, 2 = Second pick...
        public int Rank { get; private set; }
        
        // "Because you like Jazz" or "Trending Global"
        public string Explanation { get; private set; } = default!;

        // Private constructor for EF
        private RecommendationItem() { }

        // Internal constructor so only RecommendationSet can create it
        internal RecommendationItem(Guid setId, string category, double score, int rank, string explanation)
        {
            Id = Guid.NewGuid();
            RecommendationSetId = setId;
            Category = category;
            Score = score;
            Rank = rank;
            Explanation = explanation;
        }
    }
}