using Events.Domain.Repositories;
using Events.Domain.Uow;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Events.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class SendEventReminderJob(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ILogger<SendEventReminderJob> logger) : IJob
{
    private const int BatchSize = 100;

    public async Task Execute(IJobExecutionContext context)
    {
        var utcNow = DateTime.UtcNow;

        var events = await eventRepository.GetEventsDueReminderAsync(
            utcNow,
            utcNow.AddHours(24),
            BatchSize,
            context.CancellationToken);

        if (events.Count == 0)
            return;

        var triggered = 0;

        foreach (var @event in events)
        {
            var result = @event.MarkReminderTriggered(utcNow);

            if (result.IsSuccess)
            {
                triggered++;
                continue;
            }

            logger.LogWarning(
                "Failed to trigger 24h reminder for event {EventId}: {Error}",
                @event.Id,
                result.Error);
        }

        if (triggered == 0)
            return;

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Triggered 24h reminders for {TriggeredCount}/{TotalCount} event(s) at {UtcNow}.",
            triggered,
            events.Count,
            utcNow);
    }
}
