// AI.Domain/Entities/EventSnapshot.cs
using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Local read model of a published Event, owned by the AI module.
    /// Synced via integration events from the Events module.
    /// Only stores what the embedding pipeline needs — nothing else.
    /// </summary>
    public class EventSnapshot : AggregateRoot<Guid>
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime SnapshotUpdatedAt { get; private set; }
        private readonly List<string> _categories = new();
        public IReadOnlyCollection<string> Categories => _categories.AsReadOnly();

        private readonly List<string> _hashtags = new();
        public IReadOnlyCollection<string> Hashtags => _hashtags.AsReadOnly();

        private EventSnapshot() { }

        public static EventSnapshot Create(
            Guid eventId,
            string title,
            string description,
            IEnumerable<string> categories,
            IEnumerable<string> hashtags)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("EventId cannot be empty.", nameof(eventId));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            var now = DateTime.UtcNow;

            var snapshot = new EventSnapshot
            {
                Id = eventId,          // same ID as the real Event
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                IsActive = true,
                SnapshotUpdatedAt = now,
                CreatedAt = now
            };

            snapshot._categories.AddRange(
                categories
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim().ToLowerInvariant())
                    .Distinct());

            snapshot._hashtags.AddRange(
                hashtags
                    .Where(h => !string.IsNullOrWhiteSpace(h))
                    .Select(h => h.Trim().ToLowerInvariant())
                    .Distinct());

            return snapshot;
        }

        /// <summary>
        /// Called when the Event module publishes an update.
        /// Replaces all fields — snapshot is always a full overwrite, never partial.
        /// </summary>
        public void Update(
            string title,
            string description,
            IEnumerable<string> categories,
            IEnumerable<string> hashtags)
        {
            Title = title.Trim();
            Description = description?.Trim() ?? string.Empty;
            SnapshotUpdatedAt = DateTime.UtcNow;

            _categories.Clear();
            _categories.AddRange(
                categories
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim().ToLowerInvariant())
                    .Distinct());

            _hashtags.Clear();
            _hashtags.AddRange(
                hashtags
                    .Where(h => !string.IsNullOrWhiteSpace(h))
                    .Select(h => h.Trim().ToLowerInvariant())
                    .Distinct());
        }

        /// <summary>
        /// Builds the flat text that gets fed into the embedding model.
        /// Format: "{title}. {description}. {categories}. {hashtags}"
        /// </summary>
        public string ToEmbeddingText()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Title))
                parts.Add(Title);

            if (!string.IsNullOrWhiteSpace(Description))
                parts.Add(Description);

            if (_categories.Count > 0)
                parts.Add(string.Join(" ", _categories));

            if (_hashtags.Count > 0)
                parts.Add(string.Join(" ", _hashtags));

            return string.Join(". ", parts);
        }

        public void Deactivate()
        {
            IsActive = false;
            SnapshotUpdatedAt = DateTime.UtcNow;
        }

        protected override void Apply(IDomainEvent @event) { }
    }
}