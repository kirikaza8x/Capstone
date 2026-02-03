using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class MarketingContent : AggregateRoot<Guid>
    {
        public string Title { get; private set; } = default!;
        public string LangCode { get; private set; } = default!;
        public Guid PublisherId { get; private set; }
        public string Description { get; private set; } = default!;
        public string Tags { get; private set; } = default!;
        public string Status { get; private set; } = "draft"; // draft, pending, published, closed
        public Guid EventId { get; private set; }

        // Child collection
        public ICollection<MarketingAnalytics> Analytics { get; private set; } = new List<MarketingAnalytics>();

        private MarketingContent() { }

        public static MarketingContent Create(
            string title,
            string langCode,
            Guid publisherId,
            string description,
            string tags,
            Guid eventId)
        {
            return new MarketingContent
            {
                Id = Guid.NewGuid(),
                Title = title,
                LangCode = langCode,
                PublisherId = publisherId,
                Description = description,
                Tags = tags,
                Status = "draft",
                EventId = eventId
            };
        }

        // Domain behaviors
        public void Publish() => Status = "published";
        public void Close() => Status = "closed";
        public void UpdateDescription(string newDescription) => Description = newDescription;
        public void UpdateTags(string newTags) => Tags = newTags;

        // Manage analytics
        public void AddAnalytics(int views, int clicks, int conversions)
        {
            Analytics.Add(MarketingAnalytics.Create(Id, views, clicks, conversions));
        }

        public void UpdateLatestAnalytics(int views, int clicks, int conversions)
        {
            var latest = Analytics.OrderByDescending(a => a.RecordedAt).FirstOrDefault();
            if (latest != null)
            {
                latest.UpdateMetrics(views, clicks, conversions);
            }
            else
            {
                AddAnalytics(views, clicks, conversions);
            }
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Event sourcing hook if needed
        }
    }
}
