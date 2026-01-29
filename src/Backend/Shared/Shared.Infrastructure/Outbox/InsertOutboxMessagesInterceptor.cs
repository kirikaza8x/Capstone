//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Shared.Domain.DDD;
//using System.Text.Json;

//namespace Shared.Infrastructure.Outbox;

//public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
//{
//    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
//        DbContextEventData eventData,
//        InterceptionResult<int> result,
//        CancellationToken cancellationToken = default)
//    {
//        if (eventData.Context is not null)
//        {
//            await ConvertDomainEventsToOutboxMessagesAsync(eventData.Context, cancellationToken);
//        }

//        return await base.SavingChangesAsync(eventData, result, cancellationToken);
//    }

//    private static async Task ConvertDomainEventsToOutboxMessagesAsync(
//        DbContext context,
//        CancellationToken cancellationToken)
//    {
//        var aggregates = context.ChangeTracker
//            .Entries<IAggregateRoot>()
//            .Where(entry => entry.Entity.DomainEvents.Any())
//            .Select(entry => entry.Entity)
//            .ToList();

//        var domainEvents = aggregates
//            .SelectMany(aggregate =>
//            {
//                var events = aggregate.DomainEvents.ToList();
//                aggregate.ClearDomainEvents();
//                return events;
//            })
//            .ToList();

//        if (!domainEvents.Any())
//        {
//            return;
//        }

//        var outboxMessages = domainEvents
//            .Select(domainEvent => new OutboxMessage
//            {
//                Id = Guid.NewGuid(),
//                Type = domainEvent.GetType().AssemblyQualifiedName!,
//                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
//                OccurredOnUtc = DateTime.UtcNow
//            })
//            .ToList();

//        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
//    }
//}
