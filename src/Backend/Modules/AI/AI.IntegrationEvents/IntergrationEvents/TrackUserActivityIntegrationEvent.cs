using Shared.Application.Abstractions.EventBus;

namespace Shared.IntegrationEvents.AI;

public sealed record TrackUserActivityIntegrationEvent(
    Guid Id,                         // Required for base class
    DateTime OccurredOnUtc,          // Required for base class
    Guid UserId,                     // UserId
    string ActionType,
    string TargetId,
    string TargetType,
    IReadOnlyDictionary<string, string>? Metadata
) : IntegrationEvent(Id, OccurredOnUtc);

public record BehaviorLogPublishedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid LogId,
    Guid UserId,
    string ActionType,
    string TargetId,
    string TargetType,
    IReadOnlyDictionary<string, string>? Metadata,
    string CorrelationId
) : IntegrationEvent(Id, OccurredOnUtc);
