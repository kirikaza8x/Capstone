using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.DDD;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Data.Interceptors;

public sealed class ProcessDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        await ProcessEventsAsync(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task ProcessEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        while (true)
        {
            var aggregates = context.ChangeTracker
                .Entries<IAggregateRoot>()
                .Where(entry => entry.Entity.DomainEvents.Any())
                .Select(entry => entry.Entity)
                .ToList();

            if (!aggregates.Any())
            {
                break;
            }

            // 1. Collect and CLEAR the events so they don't fire twice
            var domainEvents = aggregates
                .SelectMany(aggregate => aggregate.ClearDomainEvents())
                .ToList();

            // 2. Save to Outbox for MassTransit / External Modules
            var outboxMessages = domainEvents
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = DateTime.UtcNow
                })
                .ToList();

            await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);

            // 3. Dispatch via MediatR for Internal Module logic
            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
