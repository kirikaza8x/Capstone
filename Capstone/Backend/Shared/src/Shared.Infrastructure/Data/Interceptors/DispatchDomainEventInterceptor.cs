using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.Common.DDD;

namespace Shared.Infrastructure.Data.Interceptors;

public class DispatchDomainEventInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    //  OLD: Dispatching inside SavingChanges (before commit)
    // public override InterceptionResult<int> SavingChanges(
    //     DbContextEventData eventData, 
    //     InterceptionResult<int> result)
    // {
    //     if (eventData.Context is not null)
    //     {
    //         Task.Run(() => DispatchDomainEvents(eventData.Context, CancellationToken.None))
    //             .GetAwaiter().GetResult();
    //     }
    //     return base.SavingChanges(eventData, result);
    // }

    //  NEW: Dispatch after persistence succeeds (post-commit)
    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is not null)
        {
            // synchronous dispatch after commit
            DispatchDomainEvents(eventData.Context, CancellationToken.None)
                .GetAwaiter().GetResult();
        }
        return base.SavedChanges(eventData, result);
    }

    // ❌ OLD: Dispatching inside SavingChangesAsync (before commit)
    // public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
    //     DbContextEventData eventData, 
    //     InterceptionResult<int> result, 
    //     CancellationToken cancellationToken = default)
    // {
    //     if (eventData.Context is not null)
    //     {
    //         await DispatchDomainEvents(eventData.Context, cancellationToken);
    //     }
    //     return await base.SavingChangesAsync(eventData, result, cancellationToken);
    // }

    //  NEW: Dispatch after persistence succeeds (post-commit, async)
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEvents(eventData.Context, cancellationToken);
        }
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Collect all events from all aggregates
        var domainEvents = new List<IDomainEvent>();

        foreach (var aggregate in aggregates)
        {
            // ClearDomainEvents returns the events and clears them
            var events = aggregate.ClearDomainEvents();
            domainEvents.AddRange(events);
        }

        // Publish all events
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}