using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Shared.Domain.DDD;

namespace Shared.Infrastructure.Outbox;

[DisallowConcurrentExecution]
public sealed class ProcessOutboxJob<TDbContext> : IJob
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessOutboxJob<TDbContext>> _logger;
    private readonly OutboxOptions _options;

    public ProcessOutboxJob(
        TDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<ProcessOutboxJob<TDbContext>> logger,
        IOptions<OutboxOptions> options)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var moduleName = typeof(TDbContext).Name.Replace("DbContext", "");
        _logger.LogDebug("[{Module}] Processing outbox messages", moduleName);

        var messages = await _dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ToListAsync(context.CancellationToken);

        if (!messages.Any())
        {
            return;
        }

        _logger.LogInformation(
            "[{Module}] Found {Count} outbox messages to process",
            moduleName,
            messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning(
                        "[{Module}] Unknown event type: {Type}",
                        moduleName,
                        message.Type);
                    message.Error = $"Unknown type: {message.Type}";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                if (domainEvent == null)
                {
                    _logger.LogWarning(
                        "[{Module}] Failed to deserialize event: {MessageId}",
                        moduleName,
                        message.Id);
                    message.Error = "Deserialization failed";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                await _publishEndpoint.Publish(domainEvent, context.CancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;

                _logger.LogDebug(
                    "[{Module}] Successfully processed message {MessageId} of type {Type}",
                    moduleName,
                    message.Id,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[{Module}] Error processing message {MessageId}",
                    moduleName,
                    message.Id);

                message.Error = ex.Message;
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "[{Module}] Completed processing {Count} outbox messages",
            moduleName,
            messages.Count);
    }
}
