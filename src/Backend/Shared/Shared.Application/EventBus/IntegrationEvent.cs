namespace Shared.Application.EventBus;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; }
    public DateTime OccurredOnUtc { get; }
    public abstract string EventType { get; }
    public Dictionary<string, string>? Metadata { get; init; }

    protected IntegrationEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }
}
