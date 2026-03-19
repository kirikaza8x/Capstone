using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record SeatReservedDomainEvent(Guid SeatId, Guid AreaId, Guid EventId) : DomainEvent;
