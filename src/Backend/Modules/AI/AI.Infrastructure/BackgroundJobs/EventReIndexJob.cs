using AI.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.BackgroundJobs;

/// <summary>
/// Scheduled background job that re-indexes all events into Qdrant every 6 hours.
///
/// WHY: Ensures any events that missed the integration event
/// (e.g. created before AI module was running, or failed embedding)
/// eventually get indexed. Acts as a safety net on top of the event-driven flow.
///
/// Register in DI:
///   services.AddHostedService&lt;EventReIndexJob&gt;();
/// </summary>
public sealed class EventReIndexJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventReIndexJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public EventReIndexJob(
        IServiceScopeFactory scopeFactory,
        ILogger<EventReIndexJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "EventReIndexJob started — interval: {Hours}h", _interval.TotalHours);

        // Delay on startup so app is fully ready
        await Task.Delay(TimeSpan.FromMinutes(2), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider
                    .GetRequiredService<IEventReIndexService>();

                var count = await service.ReIndexAllAsync(ct);

                _logger.LogInformation(
                    "EventReIndexJob completed — {Count} events indexed", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "EventReIndexJob failed — will retry next interval");
            }

            await Task.Delay(_interval, ct);
        }
    }
}