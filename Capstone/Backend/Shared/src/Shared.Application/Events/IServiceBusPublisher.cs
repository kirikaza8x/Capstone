namespace Shared.Application.Events;

/// <summary>
/// Service bus publisher abstraction
/// Technology-agnostic interface for publishing events
/// Implementation can be MassTransit, Azure Service Bus, AWS SQS, etc.
/// </summary>
public interface IServiceBusPublisher
{
    /// <summary>
    /// Publish a single event to the service bus
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class;

    /// <summary>
    /// Publish multiple events in batch
    /// </summary>
    Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default) 
        where TEvent : class;
}