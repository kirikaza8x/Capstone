using Shared.Domain.DDD; 

namespace AI.Domain.Event;

public sealed record CreateLogEvent(
    Guid UserId,
    string ActionType,
    string TargetId,
    string TargetType,
    IReadOnlyDictionary<string, string>? Metadata
) : DomainEventBase;