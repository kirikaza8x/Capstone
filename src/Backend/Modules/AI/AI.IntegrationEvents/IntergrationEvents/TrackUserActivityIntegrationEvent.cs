using Shared.Application.Abstractions.EventBus;

namespace Shared.IntegrationEvents.AI;

public sealed record TrackUserActivityIntegrationEvent(
    Guid Id,                         // Required for base class
    DateTime OccurredOnUtc,          // Required for base class
    Guid UserId,
    string ActionType,
    string TargetId,
    string TargetType,
    IReadOnlyDictionary<string, string>? Metadata
) : IntegrationEvent(Id, OccurredOnUtc);