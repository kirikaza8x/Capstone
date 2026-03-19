using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents
{
    /// <summary>
    /// Raised when a new event is created in the domain and requires embedding.
    /// </summary>
    public sealed record EventChangedEmbeddingDomainEvent(
        Guid TargetEventId,
        Guid OrganizerId,
        string Title,
        string Description,
        IReadOnlyList<string> Categories,
        IReadOnlyList<string> Hashtags,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? EventStartAt
    ) : DomainEvent;

    /// <summary>
    /// Raised when an existing event is updated in the domain and requires embedding refresh.
    /// </summary>
    public sealed record EventUpdatedEmbeddingDomainEvent(
        Guid TargetEventId,
        Guid OrganizerId,
        string Title,
        string Description,
        IReadOnlyList<string> Categories,
        IReadOnlyList<string> Hashtags,
        bool IsActive,
        DateTime UpdatedAt
    ) : DomainEvent;

    /// <summary>
    /// Raised when an event is published in the domain and requires embedding generation.
    /// </summary>
    public sealed record EventPublishedEmbeddingDomainEvent(
        Guid TargetEventId,
        Guid OrganizerId,
        string Title,
        string Description,
        IReadOnlyList<string> Categories,
        IReadOnlyList<string> Hashtags,
        bool IsActive,
        DateTime UpdatedAt
    ) : DomainEvent;
}
