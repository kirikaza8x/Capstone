namespace Shared.Application.Abstractions.EventBus;

public interface IIntegrationEvent
{
    public Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
