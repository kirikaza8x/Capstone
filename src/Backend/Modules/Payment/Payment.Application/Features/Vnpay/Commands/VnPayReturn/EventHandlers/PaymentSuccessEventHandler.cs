using Microsoft.Extensions.Logging;
using Payment.IntegrationEvents;
using Payments.Domain.Events;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.EventHandlers;

public sealed class PaymentSuccessDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentSuccessDomainEventHandler> logger)
    : IDomainEventHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(
        PaymentSucceededDomainEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await eventBus.PublishAsync(
                new PaymentSuccessIntegrationEvent(
                    id: Guid.NewGuid(),
                    occurredOnUtc: DateTime.UtcNow,
                    orderId: @event.OrderId,
                    amount: @event.Amount,
                    paidAtUtc: @event.CompletedAtUtc),
                cancellationToken);

            logger.LogInformation(
                "PaymentSuccessIntegrationEvent published: OrderId={OrderId}, Amount={Amount}, PaidAtUtc={PaidAtUtc}",
                @event.OrderId,
                @event.Amount,
                @event.CompletedAtUtc);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish PaymentSuccessIntegrationEvent: OrderId={OrderId}",
                @event.OrderId);
        }
    }
}
