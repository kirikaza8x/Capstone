using Shared.Domain.DDD;

namespace AI.Domain.Events
{
    /// <summary>
    /// Raised when a new behavior log entry is created.
    /// Payload is fully typed — consumers never re-parse raw strings.
    /// </summary>
    public sealed record BehaviorLogCreatedEvent(
    Guid LogId,
    Guid UserId,
    string ActionType,
    string TargetId,
    string TargetType,
    DateTime OccurredAt,
    IReadOnlyDictionary<string, string>? Metadata
) : DomainEventBase;
    /// <summary>
    /// Raised when a user interest score is updated (after decay + new points).
    /// </summary>
    public sealed record InterestScoreUpdatedEvent(
        Guid ScoreId,
        Guid UserId,
        string Category,
        double PreviousScore,
        double NewScore,
        DateTime UpdatedAt
    ) : DomainEventBase;

    /// <summary>
    /// Raised when an interaction weight is deactivated (e.g. before rolling out a new version).
    /// </summary>
    public sealed record InteractionWeightDeactivatedEvent(
        Guid WeightId,
        string ActionType,
        string Version,
        DateTime DeactivatedAt
    ) : DomainEventBase;

    /// <summary>
    /// Raised when a user embedding is recalculated.
    /// Signals downstream services to refresh ANN indexes if needed.
    /// </summary>
    public sealed record UserEmbeddingRecalculatedEvent(
        Guid UserId,
        int Dimension,
        int ContributingCategoryCount,
        double Confidence,
        DateTime CalculatedAt
    ) : DomainEventBase;

    /// <summary>
    /// Raised when a category description changes and embedding regeneration is needed.
    /// A handler should pick this up and call the embedding service.
    /// </summary>
    public sealed record CategoryDescriptionChangedEvent(
        Guid CategoryEmbeddingId,
        string Category,
        string NewDescription,
        DateTime ChangedAt
    ) : DomainEventBase;
}