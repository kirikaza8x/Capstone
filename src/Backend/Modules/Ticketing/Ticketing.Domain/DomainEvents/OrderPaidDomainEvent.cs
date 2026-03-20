using Shared.Domain.DDD;

namespace Ticketing.Domain.DomainEvents;

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalPrice) : DomainEvent;
