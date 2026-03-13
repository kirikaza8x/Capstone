using Microsoft.Extensions.Logging;
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
            var integrationEvent =
                new TrackUserActivityIntegrationEvent(
                    notification.LogId,
                    notification.OccurredAt,
                    notification.UserId,
                    notification.ActionType,
                    notification.TargetId,
                    notification.TargetType,
                    notification.Metadata);

            await _eventBus.PublishAsync(integrationEvent, ct);
        }
    }
}