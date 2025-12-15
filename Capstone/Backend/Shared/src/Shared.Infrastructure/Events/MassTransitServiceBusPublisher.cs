using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Application.Events;

namespace Shared.Infrastructure.Events;

/// <summary>
/// MassTransit implementation of IServiceBusPublisher
/// Handles publishing events to RabbitMQ
/// </summary>
public class MassTransitServiceBusPublisher : IServiceBusPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitServiceBusPublisher> _logger;

    public MassTransitServiceBusPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitServiceBusPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            _logger.LogInformation("Publishing event: {EventType}", typeof(TEvent).Name);
            await _publishEndpoint.Publish(@event, cancellationToken);
            _logger.LogInformation("Event published successfully: {EventType}", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            var eventList = events.ToList();
            _logger.LogInformation("Publishing {Count} events of type: {EventType}", eventList.Count, typeof(TEvent).Name);

            foreach (var @event in eventList)
            {
                await _publishEndpoint.Publish(@event, cancellationToken);
            }

            _logger.LogInformation("All events published successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch of events: {EventType}", typeof(TEvent).Name);
            throw;
        }
    }
}
