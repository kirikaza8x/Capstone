using Events.Domain.Repositories;
using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Quartz;
using Shared.Application.Abstractions.EventBus;

namespace Events.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class NotifySuspensionExpiredEventsJob(
    IEventRepository eventRepository,
    IEventBus eventBus,
    ILogger<NotifySuspensionExpiredEventsJob> logger) : IJob
{
    private const int BatchSize = 50;

    public async Task Execute(IJobExecutionContext context)
    {
        var utcNow = DateTime.UtcNow;
        var fromUtc = utcNow.AddMinutes(-15);

        var events = await eventRepository.GetSuspendedExpiredEventsAsync(
            fromUtc,
            utcNow,
            BatchSize,
            context.CancellationToken);

        foreach (var @event in events)
        {
            if (!@event.SuspendedBy.HasValue || !@event.SuspendedUntilAt.HasValue)
                continue;

            var integrationEvent = new EventSuspensionExpiredIntegrationEvent(
                id: Guid.NewGuid(),
                occurredOnUtc: utcNow,
                eventId: @event.Id,
                suspendedBy: @event.SuspendedBy.Value,
                eventTitle: @event.Title,
                suspendedUntilAtUtc: @event.SuspendedUntilAt.Value);

            await eventBus.PublishAsync(integrationEvent, context.CancellationToken);
        }

        logger.LogInformation("Published suspension-expired notifications for {Count} event(s).", events.Count);
    }
}
