using AI.PublicApi.Enums;
using Shared.Application.Abstractions.EventBus;

namespace AI.IntegrationEvents.IntergrationEvents;

public sealed record TrackUserActivityIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid UserId,
    string ActionType,
    string TargetId,
    string TargetType,
    IReadOnlyDictionary<string, string>? Metadata
) : IntegrationEvent(Id, OccurredOnUtc)
{
    public static TrackUserActivityIntegrationEvent Create(
        Guid userId,
        string actionType,
        string targetId,
        TargetType targetType,
        IReadOnlyDictionary<string, string>? metadata = null,
        DateTime? occurredOnUtc = null,
        Guid? id = null)
    {
        if (!ActionTypes.IsKnown(actionType))
            throw new ArgumentException($"Unknown action type: {actionType}", nameof(actionType));

        return new TrackUserActivityIntegrationEvent(
            Id: id ?? Guid.NewGuid(),
            OccurredOnUtc: occurredOnUtc ?? DateTime.UtcNow,
            UserId: userId,
            ActionType: actionType,
            TargetId: targetId,
            TargetType: targetType.ToValue(),
            Metadata: metadata);
    }
}


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
