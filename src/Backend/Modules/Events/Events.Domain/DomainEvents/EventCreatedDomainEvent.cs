using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventCreatedDomainEvent(
    Guid AggregateEventId,
    Guid OrganizerId) : DomainEvent;
