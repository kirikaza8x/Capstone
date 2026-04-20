using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.DDD;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Data.Interceptors;

public sealed class ProcessDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    private readonly List<IDomainEvent> _pendingEvents = new();

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
        if (_pendingEvents.Any())
        {
            var eventsToDispatch = _pendingEvents.ToList();
            _pendingEvents.Clear();

            foreach (var domainEvent in eventsToDispatch)
                await mediator.Publish(domainEvent, cancellationToken);
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

        // Stage for dispatch after save
        _pendingEvents.AddRange(domainEvents);

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