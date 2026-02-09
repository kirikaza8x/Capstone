using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventPublishedDomainEvent(Guid EventId) : DomainEvent;