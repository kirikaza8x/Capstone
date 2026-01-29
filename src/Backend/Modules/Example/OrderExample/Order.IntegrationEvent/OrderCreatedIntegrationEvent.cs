using Shared.Application.EventBus;

namespace Order.IntegrationEvents;

public sealed class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; }
    public List<OrderItemDto> Items { get; }

    public OrderCreatedIntegrationEvent(Guid orderId, List<OrderItemDto> items)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        OrderId = orderId;
        Items = items;
    }
}

public sealed class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}