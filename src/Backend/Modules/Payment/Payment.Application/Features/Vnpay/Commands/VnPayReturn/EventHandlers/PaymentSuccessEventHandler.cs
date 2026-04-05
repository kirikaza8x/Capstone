using Microsoft.Extensions.Logging;
using Payment.Domain.Events;
using Payment.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using DomainPaymentReferenceType = Payment.Domain.Enums.PaymentReferenceType;
using IntegrationPaymentReferenceType = Payment.IntegrationEvents.PaymentReferenceType;

namespace Payment.Application.Features.Vnpay.Commands.VnPayReturn.EventHandlers;

public sealed class PaymentSuccessDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentSuccessDomainEventHandler> logger)
    : IDomainEventHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(
        PaymentSucceededDomainEvent @event,
        CancellationToken cancellationToken)
    {
        try
        {
            var integrationReferenceType = @event.ReferenceType switch
            {
                DomainPaymentReferenceType.TicketOrder => IntegrationPaymentReferenceType.TicketOrder,
                DomainPaymentReferenceType.AiPackage => IntegrationPaymentReferenceType.AiPackage,
                _ => IntegrationPaymentReferenceType.TicketOrder
            };

            await eventBus.PublishAsync(
                new PaymentSuccessIntegrationEvent(
                    Id: Guid.NewGuid(),
                    OccurredOnUtc: DateTime.UtcNow,
                    PaymentTransactionId: @event.PaymentTransactionId,
                    UserId: @event.UserId,
                    ReferenceType: integrationReferenceType,
                    ReferenceId: @event.ReferenceId,
                    Amount: @event.Amount,
                    PaidAtUtc: @event.CompletedAtUtc,
                    OrderId: @event.OrderId ?? Guid.Empty),
                cancellationToken);

            logger.LogInformation(
                "PaymentSuccessIntegrationEvent published: TxnId={TxnId}, UserId={UserId}, RefType={RefType}, RefId={RefId}, Amount={Amount}",
                @event.PaymentTransactionId,
                @event.UserId,
                @event.ReferenceType,
                @event.ReferenceId,
                @event.Amount);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish PaymentSuccessIntegrationEvent: TxnId={TxnId}",
                @event.PaymentTransactionId);
        }
    }
}
