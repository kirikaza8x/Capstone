using Shared.Domain.DDD;

namespace Order.Domain.Orders.Events;

public sealed record OrderCancelledDomainEvent(Guid OrderId) : DomainEvent;
