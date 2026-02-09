using Shared.Domain.DDD;

namespace AI.Domain.ReadModels
{
    public class GlobalCategoryStat : AggregateRoot<Guid>
    {
        public string Category { get; private set; } = default!;
        
        /// <summary>
        /// How popular is this category globally? (normalized 0-100 or 0-1.0)
        /// </summary>
        public double PopularityScore { get; private set; }
        
        // Total interactions count (Used to calculate confidence / Bayesian C factor)
        public int TotalInteractions { get; private set; }
        
        // Audit field: Helps you debug "Why is this category popular?"
        public DateTime LastCalculated { get; private set; }

        // EF Core requires a private constructor
        private GlobalCategoryStat() { }

        // Factory Method
        public static GlobalCategoryStat Create(string category, double score, int count)
        {
            return new GlobalCategoryStat
            {
                Id = Guid.NewGuid(),
                Category = category.ToLowerInvariant().Trim(), // Always normalize keys
                PopularityScore = score,
                TotalInteractions = count,
                LastCalculated = DateTime.UtcNow
            };
        }

        // Method to update stats (called by Background Job)
        public void UpdateStats(double newScore, int newTotalCount)
        {
            PopularityScore = newScore;
            TotalInteractions = newTotalCount;
            LastCalculated = DateTime.UtcNow;
        }

        protected override void Apply(IDomainEvent @event) { }
    }
}