namespace Shared.Application.EventBus;

public interface IIntegrationEvent
{
    public Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}