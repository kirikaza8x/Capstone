using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Domain.DDD;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Data.Interceptors;

public sealed class ProcessDomainEventsInterceptor(
    IMediator mediator,
    ILogger<ProcessDomainEventsInterceptor> _logger) : SaveChangesInterceptor
{
    private readonly List<IDomainEvent> _pendingEvents = [];
    private readonly HashSet<Guid> _publishedEventIds = [];

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await CollectEventsAndSaveOutboxAsync(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingEvents.Count > 0)
        {
            // Filter out events that have already been published to prevent duplicate dispatch
            var eventsToDispatch = _pendingEvents
                .Where(e => !_publishedEventIds.Contains(e.EventId))
                .ToList();

            // Mark these events as published
            foreach (var e in eventsToDispatch)
            {
                _publishedEventIds.Add(e.EventId);
            }

            if (eventsToDispatch.Count > 0)
            {
                _logger.LogInformation(
                    "SavedChangesAsync: Publishing {Count} domain events. Total pending: {Total}, Already published: {AlreadyPublished}",
                    eventsToDispatch.Count, _pendingEvents.Count, _publishedEventIds.Count);

                foreach (var domainEvent in eventsToDispatch)
                {
                    _logger.LogInformation(
                        "  Publishing event: Type={Type}, EventId={EventId}",
                        domainEvent.GetType().Name, domainEvent.EventId);
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            _pendingEvents.Clear();
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task CollectEventsAndSaveOutboxAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        if (!aggregates.Any()) return;

        var domainEvents = aggregates
            .SelectMany(a => a.ClearDomainEvents())
            .ToList();

        // Add events to pending list (deduplication will happen at publish time)
        _pendingEvents.AddRange(domainEvents);

        _logger.LogInformation(
            "CollectEventsAndSaveOutbox: Collected {Count} events from {AggregateCount} aggregates. Total pending: {Total}",
            domainEvents.Count, aggregates.Count, _pendingEvents.Count);

        foreach (var e in domainEvents)
        {
            _logger.LogInformation(
                "  Collected event: Type={Type}, EventId={EventId}",
                e.GetType().Name, e.EventId);
        }

        // Save to Outbox in same transaction
        var outboxMessages = domainEvents
            .Select(e => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = e.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(e, e.GetType()),
                OccurredOnUtc = DateTime.UtcNow
            })
            .ToList();

        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }
}