using Shared.Application.EventBus;

namespace Products.IntegrationEvents;

public sealed class ProductUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }

    public ProductUpdatedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid productId,
        string name,
        decimal price,
        int stock) : base(id, occurredOnUtc)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        Stock = stock;
    }
}
