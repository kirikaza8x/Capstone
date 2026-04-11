namespace Shared.Application.Abstractions.EventBus;

public abstract class IntegrationEventHandler<TIntegrationEvent> : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : class, IIntegrationEvent
{
    public abstract Task Handle(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
