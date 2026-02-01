using Shared.Domain.DDD;

namespace Order.Domain.Orders.Events;

public sealed record OrderCreatedDomainEvent(Guid OrderId, List<OrderItem> Items) : DomainEvent;