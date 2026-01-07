using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Shared.Application.EventBus;
using System.Text.Json;

namespace Shared.Infrastructure.Inbox;

[DisallowConcurrentExecution]
public sealed class ProcessInboxJob<TDbContext> : IJob
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessInboxJob<TDbContext>> _logger;
    private readonly InboxOptions _options;

    public ProcessInboxJob(
        TDbContext dbContext,
        IServiceProvider serviceProvider,
        ILogger<ProcessInboxJob<TDbContext>> logger,
        IOptions<InboxOptions> options)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var moduleName = typeof(TDbContext).Name.Replace("DbContext", "");
        await RetryFailedMessagesAsync(moduleName, context.CancellationToken);
        await CleanupOldMessagesAsync(moduleName, context.CancellationToken);
    }

    private async Task RetryFailedMessagesAsync(string moduleName, CancellationToken cancellationToken)
    {
        var failedMessages = await _dbContext.Set<InboxMessage>()
            .Where(m => m.Error != null && m.ProcessedOnUtc == null)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (!failedMessages.Any())
        {
            return;
        }

        _logger.LogInformation(
            "[{Module}] Retrying {Count} failed inbox messages",
            moduleName,
            failedMessages.Count);

        foreach (var message in failedMessages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("[{Module}] Unknown event type: {Type}", moduleName, message.Type);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize(message.Content, eventType);

                // Get handler from DI
                var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                var handler = _serviceProvider.GetService(handlerType);

                if (handler == null)
                {
                    _logger.LogWarning("[{Module}] No handler for {Type}", moduleName, message.Type);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                // Invoke handler
                var handleMethod = handlerType.GetMethod("Handle");
                await (Task)handleMethod!.Invoke(handler, new[] { integrationEvent, cancellationToken })!;

                // Success
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation("[{Module}] Successfully retried message {Id}", moduleName, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Module}] Retry failed for message {Id}", moduleName, message.Id);
                message.Error = ex.Message;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CleanupOldMessagesAsync(string moduleName, CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);

        var oldMessages = await _dbContext.Set<InboxMessage>()
            .Where(m => m.ProcessedOnUtc < cutoffDate)
            .ToListAsync(cancellationToken);

        if (!oldMessages.Any())
        {
            return;
        }

        _logger.LogInformation(
            "[{Module}] Cleaning up {Count} old inbox messages",
            moduleName,
            oldMessages.Count);

        _dbContext.Set<InboxMessage>().RemoveRange(oldMessages);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
