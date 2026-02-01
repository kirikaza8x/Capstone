using MediatR;
using Order.IntegrationEvents;
using Products.Application.Products.Commands.ReduceStockCommand;
using Products.Domain.Products;
using Shared.Application.EventBus;

namespace Products.Application.Products.EventHandlers.IntergrationEvents;

public class OrderCreatedEventHandler : IntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderCreatedEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public override async Task Handle(OrderCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var items = integrationEvent.Items
            .Select(i => new ReduceStockItem(i.ProductId, i.Quantity))
            .ToList();

        var command = new ReduceStockCommand(items);
        await _sender.Send(command, cancellationToken);
    }
}
