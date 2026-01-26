using Shared.Domain.DDD;

namespace Order.Domain.Orders.Events;

public sealed record OrderConfirmedDomainEvent(Guid OrderId) : DomainEvent;