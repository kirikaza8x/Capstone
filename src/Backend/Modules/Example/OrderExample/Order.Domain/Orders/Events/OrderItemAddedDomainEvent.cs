using Shared.Domain.DDD;
namespace Order.Domain.Orders.Events;

public sealed record OrderItemAddedDomainEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : DomainEvent;
