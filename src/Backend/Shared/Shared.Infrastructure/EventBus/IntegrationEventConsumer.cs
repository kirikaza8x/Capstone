using MassTransit;
using Shared.Application.EventBus;

namespace Shared.Infrastructure.EventBus;

public class IntegrationEventConsumer<TIntegrationEvent> : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : class, IIntegrationEvent
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _handler;

    public IntegrationEventConsumer(IIntegrationEventHandler<TIntegrationEvent> handler)
    {
        _handler = handler;
    }

    public Task Consume(ConsumeContext<TIntegrationEvent> context) =>
        _handler.Handle(context.Message, context.CancellationToken);
}