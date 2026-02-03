using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class RecommendationSet : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public DateTime GeneratedAt { get; private set; }
        public ICollection<RecommendationItem> Items { get; private set; } = new List<RecommendationItem>();

        private RecommendationSet() { }

        public static RecommendationSet Create(Guid userId)
        {
            return new RecommendationSet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GeneratedAt = DateTime.UtcNow
            };
        }

        // Domain behavior
        public void AddItem(Guid eventId, double score, string? explanation = null)
        {
            Items.Add(new RecommendationItem(eventId, score, explanation));
        }

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
                
            // }
        }
    }


}
