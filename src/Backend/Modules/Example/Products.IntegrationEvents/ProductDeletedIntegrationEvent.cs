using Shared.Application.EventBus;

namespace Products.IntegrationEvents;

public sealed class ProductDeletedIntegrationEvent : IntegrationEvent
{
    public Guid ProductId { get; init; }

    public ProductDeletedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid productId) : base(id, occurredOnUtc)
    {
        ProductId = productId;
    }
}
