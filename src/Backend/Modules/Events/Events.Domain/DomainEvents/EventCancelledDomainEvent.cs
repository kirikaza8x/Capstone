using Shared.Domain.DDD;

namespace Events.Domain.DomainEvents;

public sealed record EventCancelledDomainEvent(Guid EventId, string? CancellationReason) : DomainEvent;
