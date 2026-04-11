using MassTransit;
using Shared.Application.Abstractions.EventBus;

namespace Shared.Infrastructure.EventBus;

public class IntegrationEventConsumer<TIntegrationEvent> : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : class, IIntegrationEvent
{
    private readonly IEnumerable<IIntegrationEventHandler<TIntegrationEvent>> _handlers;

    public IntegrationEventConsumer(IEnumerable<IIntegrationEventHandler<TIntegrationEvent>> handlers)
    {
        _handlers = handlers;
    }

    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        foreach (var handler in _handlers)
        {
            await handler.Handle(context.Message, context.CancellationToken);
        }
    }
}
