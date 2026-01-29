using Order.IntegrationEvents;
using Products.Domain.Products;
using Shared.Application.EventBus;

namespace Products.Application.Products.EventHandlers.IntergrationEvents;

public class OrderCreatedEventConsumer : IntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IProductRepository _productRepository;

    public OrderCreatedEventConsumer(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public override async Task Handle(OrderCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        foreach (var item in integrationEvent.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is not null && product.Stock >= item.Quantity)
            {
                product.UpdateStock(-item.Quantity);
                _productRepository.Update(product);
            }
        }
    }
}
