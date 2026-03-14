using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using AI.Domain.Events;
using Shared.IntegrationEvents.AI;

namespace AI.Application.Features.Tracking.EventHandlers
{
    public sealed class BehaviorLogCreatedEventHandler
    : IDomainEventHandler<BehaviorLogCreatedEvent>
    {
        private readonly IEventBus _eventBus;

        public BehaviorLogCreatedEventHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task Handle(
            BehaviorLogCreatedEvent notification,
            CancellationToken ct)
        {
           var outboundEvent = new BehaviorLogPublishedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOnUtc: DateTime.UtcNow,
            LogId: notification.LogId,
            UserId: notification.UserId,
            ActionType: notification.ActionType,
            TargetId: notification.TargetId,
            TargetType: notification.TargetType,
            Metadata: notification.Metadata,
            CorrelationId: Guid.NewGuid().ToString()
        );
            await _eventBus.PublishAsync(outboundEvent, ct);
        }
    }
}