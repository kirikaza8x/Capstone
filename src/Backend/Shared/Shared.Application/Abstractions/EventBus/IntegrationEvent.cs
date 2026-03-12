namespace Shared.Application.Abstractions.EventBus;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; }
    public DateTime OccurredOnUtc { get; }

    protected IntegrationEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }
}
