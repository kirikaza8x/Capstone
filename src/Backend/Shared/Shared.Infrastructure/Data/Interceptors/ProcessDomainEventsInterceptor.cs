using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.DDD;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Data.Interceptors;

public sealed class ProcessDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    // INSTANCE-SCOPED LIST
    // Shared between SavingChangesAsync and SavedChangesAsync within one SaveChanges call.
    // This is the bridge that connects DISPATCH #1 setup → DISPATCH #1 fire.
    // If interceptor is Singleton: this bleeds across requests (race conditions).
    // If interceptor is Scoped: safe, but the double-dispatch problem still exists.
    private readonly List<IDomainEvent> _pendingEvents = new();

    // ═══════════════════════════════════════════════════════════════
    // PRE-SAVE PHASE
    // Runs BEFORE EF writes anything to the database.
    // This is where BOTH dispatches are SET UP in the same call.
    // ═══════════════════════════════════════════════════════════════
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            // BOTH DISPATCH #1 AND DISPATCH #2 ARE SET UP INSIDE THIS CALL
            // After this returns, every domain event exists in two places simultaneously:
            //   → _pendingEvents  (DISPATCH #1 — fired in SavedChangesAsync below)
            //   → OutboxMessages  (DISPATCH #2 — fired later by outbox background job)
            await CollectEventsAndSaveOutboxAsync(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════
    // POST-SAVE PHASE — DISPATCH #1 FIRES HERE
    // Runs AFTER EF has successfully written to the database.
    // ═══════════════════════════════════════════════════════════════
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
                // DISPATCH #1 FIRES HERE
                // Timeline: immediate — still inside the same HTTP request scope
                // Effect: NotifyN8nOfQueuedDistributionHandler runs right now
                //         → n8n webhook is called → WEBHOOK CALL #1
                // Problem: the exact same event is already persisted in OutboxMessages
                //          table from SavingChangesAsync above, so DISPATCH #2
                //          will repeat this exact publish later
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

        // DISPATCH #1 SETUP — loads events into memory
        // These will be published immediately in SavedChangesAsync (post-save, same request)
        // Timeline: fires within milliseconds after EF saves
        _pendingEvents.AddRange(domainEvents);

        // DISPATCH #2 SETUP — persists events to Outbox table
        // The outbox background job will later read these rows and call mediator.Publish()
        // Timeline: fires after background job polling interval (seconds/minutes later)
        // This is the correct and durable path — DISPATCH #1 above is the redundant one
        var outboxMessages = domainEvents
            .Select(e => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = e.GetType().AssemblyQualifiedName!,
                // Full event serialized here — when outbox processor deserializes
                // and republishes this later, it is identical to DISPATCH #1 above.
                // No dedup exists at this layer — guaranteed duplicate handler execution.
                Content = JsonSerializer.Serialize(e, e.GetType()),
                OccurredOnUtc = DateTime.UtcNow
            })
            .ToList();

        // DISPATCH #2 SETUP COMPLETES HERE
        // After this line the event row exists in DB within the same EF transaction.
        // Once EF commits, the outbox processor can pick it up and fire DISPATCH #2.
        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }
}