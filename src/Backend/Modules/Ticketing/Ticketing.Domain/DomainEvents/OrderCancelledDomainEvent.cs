using Shared.Domain.DDD;

namespace Ticketing.Domain.DomainEvents;

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    Guid UserId) : DomainEvent;