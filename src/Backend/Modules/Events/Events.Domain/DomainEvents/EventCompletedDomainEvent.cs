using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventCompletedDomainEvent(
    Guid AggregateEventId) : DomainEvent;
