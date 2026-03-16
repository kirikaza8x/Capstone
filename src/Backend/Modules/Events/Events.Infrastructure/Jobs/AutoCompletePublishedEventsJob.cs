using Events.Domain.Repositories;
using Events.Domain.Uow;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Events.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class AutoCompletePublishedEventsJob(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ILogger<AutoCompletePublishedEventsJob> logger) : IJob
{
    private const int BatchSize = 50;

    public async Task Execute(IJobExecutionContext context)
    {
        var utcNow = DateTime.UtcNow;

        var events = await eventRepository.GetPublishedEndedEventsAsync(
            utcNow,
            BatchSize,
            context.CancellationToken);

        if (events.Count == 0)
            return;

        var completedCount = 0;

        foreach (var @event in events)
        {
            var result = @event.Complete(utcNow);

            if (result.IsSuccess)
                completedCount++;
            else
                logger.LogWarning(
                    "Failed to complete event {EventId}: {Error}",
                    @event.Id,
                    result.Error);
        }

        if (completedCount == 0)
            return;

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Auto completed {CompletedCount}/{Total} event(s) at {UtcNow}.",
            completedCount,
            events.Count,
            utcNow);
    }
}