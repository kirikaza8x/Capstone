using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventCreatedDomainEvent(Guid EventId, Guid OrganizerId) : DomainEvent;
