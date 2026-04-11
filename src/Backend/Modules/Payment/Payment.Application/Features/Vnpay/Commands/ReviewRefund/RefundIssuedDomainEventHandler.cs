using Microsoft.Extensions.Logging;
using Payment.IntegrationEvents;
using Payments.Domain.Events;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.EventHandlers;

public sealed class RefundIssuedDomainEventHandler(
    IEventBus eventBus,
    ILogger<RefundIssuedDomainEventHandler> logger)
    : IDomainEventHandler<RefundIssuedDomainEvent>
{
    public async Task Handle(
        RefundIssuedDomainEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await eventBus.PublishAsync(
                new RefundIssuedIntegrationEvent(
                    id: Guid.NewGuid(),
                    occurredOnUtc: DateTime.UtcNow,
                    orderId: @event.OrderId,
                    orderTicketId: @event.OrderTicketId,
                    eventSessionId: @event.EventSessionId,
                    userId: @event.UserId,
                    amount: @event.Amount,
                    refundedAtUtc: @event.RefundedAtUtc),
                cancellationToken);

            logger.LogInformation(
                "RefundIssuedIntegrationEvent published: " +
                "OrderTicketId={OrderTicketId}, UserId={UserId}, Amount={Amount}",
                @event.OrderTicketId, @event.UserId, @event.Amount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish RefundIssuedIntegrationEvent: " +
                "OrderTicketId={OrderTicketId}",
                @event.OrderTicketId);
        }
    }
}
