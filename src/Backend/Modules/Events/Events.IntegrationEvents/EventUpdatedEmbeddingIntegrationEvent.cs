using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents
{


    /// <summary>
    /// Published when an existing event is updated and needs its embedding refreshed.
    /// AI module consumes this to update the vector in Qdrant.
    /// </summary>
    public sealed record EventUpdatedEmbeddingIntegrationEvent : IntegrationEvent
    {
        public Guid EventId { get; init; }
        public Guid OrganizerId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public IReadOnlyList<string> Categories { get; init; } = [];
        public IReadOnlyList<string> Hashtags { get; init; } = [];
        public bool IsActive { get; init; }
        public DateTime UpdatedAt { get; init; }

        public EventUpdatedEmbeddingIntegrationEvent(
            Guid eventId,
            Guid organizerId,
            string title,
            string description,
            IReadOnlyList<string> categories,
            IReadOnlyList<string> hashtags,
            bool isActive,
            DateTime updatedAt)
            : base(Guid.NewGuid(), DateTime.UtcNow)
        {
            EventId = eventId;
            OrganizerId = organizerId;
            Title = title;
            Description = description;
            Categories = categories;
            Hashtags = hashtags;
            IsActive = isActive;
            UpdatedAt = updatedAt;
        }
    }
}
