using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventUnpublishedDomainEvent(Guid EventId) : DomainEvent;
