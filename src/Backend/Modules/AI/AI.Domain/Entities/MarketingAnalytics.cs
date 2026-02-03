using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class MarketingAnalytics : Entity<Guid>
    {
        public Guid MarketingContentId { get; private set; }
        public int Views { get; private set; }
        public int Clicks { get; private set; }
        public int Conversions { get; private set; }
        public DateTime RecordedAt { get; private set; }

        private MarketingAnalytics() { }

        public static MarketingAnalytics Create(Guid marketingContentId, int views, int clicks, int conversions)
        {
            return new MarketingAnalytics
            {
                Id = Guid.NewGuid(),
                MarketingContentId = marketingContentId,
                Views = views,
                Clicks = clicks,
                Conversions = conversions,
                RecordedAt = DateTime.UtcNow
            };
        }

        // Domain behaviors
        public void UpdateMetrics(int views, int clicks, int conversions)
        {
            Views = views;
            Clicks = clicks;
            Conversions = conversions;
            RecordedAt = DateTime.UtcNow;
        }

        public double ClickThroughRate =>
            Views == 0 ? 0 : (double)Clicks / Views;

        public double ConversionRate =>
            Clicks == 0 ? 0 : (double)Conversions / Clicks;
    }
}
