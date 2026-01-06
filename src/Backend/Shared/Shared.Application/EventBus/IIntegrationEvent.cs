namespace Shared.Application.EventBus;

public interface IIntegrationEvent
{
    public Guid Id { get; }
    DateTime OccurredOnUtc { get; }
    string EventType { get; }
    public Dictionary<string, string>? Metadata { get; init; }
}