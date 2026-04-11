using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventCancelledDomainEvent(
    Guid AggregateEventId,
    string? CancellationReason) : DomainEvent;
