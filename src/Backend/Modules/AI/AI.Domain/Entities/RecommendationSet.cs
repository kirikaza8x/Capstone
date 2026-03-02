using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class RecommendationSet : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string Type { get; private set; } = default!; // e.g. "HomeFeed", "DailyMix"
        public DateTime GeneratedAt { get; private set; }

        // Navigation Property
        private readonly List<RecommendationItem> _items = new();
        public IReadOnlyCollection<RecommendationItem> Items => _items.AsReadOnly();

        private RecommendationSet() { }

        public static RecommendationSet Create(Guid userId, string type)
        {
            return new RecommendationSet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public void AddItem(string category, double score, string explanation)
        {
            // Rank is automatically determined by the order you add them
            int rank = _items.Count + 1;
            _items.Add(new RecommendationItem(Id, category, score, rank, explanation));
        }

        public void ClearItems()
        {
            _items.Clear();
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event application logic if needed
        }
    }
}