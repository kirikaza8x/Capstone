using Order.Domain.Orders.Events;
using Order.IntegrationEvents;
using Shared.Application.EventBus;
using Shared.Application.Messaging;

namespace Order.Application.Orders.EventHandlers.Domain;

public class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public OrderCreatedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Map domain items sang DTO
        var items = notification.Items
            .Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            })
            .ToList();

        var integrationEvent = new OrderCreatedIntegrationEvent(
            notification.OrderId,
            items
        );
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}