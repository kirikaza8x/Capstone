using AI.IntegrationEvents.IntergrationEvents;
using AI.PublicApi.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Ticketing.Application.Orders.Commands.ConfirmOrder;

namespace Ticketing.Application.Orders.EventHandlers;

public class PaymentSuccessIntegrationEventHandler(
    ISender sender,
    IEventBus eventBus,
    ILogger<PaymentSuccessIntegrationEventHandler> logger)
    : IntegrationEventHandler<PaymentSuccessIntegrationEvent>
{
    public override async Task Handle(
        PaymentSuccessIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (integrationEvent.ReferenceType != PaymentReferenceType.TicketOrder)
            return;

        var orderId = integrationEvent.ReferenceId != Guid.Empty
            ? integrationEvent.ReferenceId
            : integrationEvent.OrderId;

        if (orderId == Guid.Empty)
        {
            logger.LogWarning(
                "PaymentSuccess received for TicketOrder but OrderId is empty. TxnId={TxnId}",
                integrationEvent.PaymentTransactionId);
            return;
        }

        var result = await sender.Send(
            new ConfirmOrderCommand(
                orderId,
                integrationEvent.Amount,
                integrationEvent.PaidAtUtc),
            cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "ConfirmOrder failed. OrderId={OrderId}, Error={ErrorCode}-{ErrorDescription}",
                orderId,
                result.Error.Code,
                result.Error.Description);
            return;
        }

        var purchaseEvent = TrackUserActivityIntegrationEvent.Create(
            userId: integrationEvent.UserId,
            actionType: ActionTypes.Purchase,
            targetId: orderId.ToString(),
            targetType: TargetType.Ticket,
            metadata: new Dictionary<string, string>
            {
                ["paymentTransactionId"] = integrationEvent.PaymentTransactionId.ToString(),
                ["amount"] = integrationEvent.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });

        await eventBus.PublishAsync(purchaseEvent, cancellationToken);
    }
}
