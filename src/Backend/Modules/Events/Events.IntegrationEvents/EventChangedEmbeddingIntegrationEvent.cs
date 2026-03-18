using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

/// <summary>
/// Published when an event is created or updated and needs re-embedding.
/// AI module consumes this to generate + store the vector in Qdrant.
/// </summary>
public sealed record EventChangedEmbeddingIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; init; }
    public Guid OrganizerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<string> Hashtags { get; init; } = [];
    public bool IsActive { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? EventStartAt { get; init; }

    public EventChangedEmbeddingIntegrationEvent(
        Guid eventId,
        Guid organizerId,
        string title,
        string description,
        IReadOnlyList<string> categories,
        IReadOnlyList<string> hashtags,
        bool isActive,
        DateTime? createdAt,
        DateTime? eventStartAt = null)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        Title = title;
        Description = description;
        Categories = categories;
        Hashtags = hashtags;
        IsActive = isActive;
        CreatedAt = createdAt;
        EventStartAt = eventStartAt;
    }
}