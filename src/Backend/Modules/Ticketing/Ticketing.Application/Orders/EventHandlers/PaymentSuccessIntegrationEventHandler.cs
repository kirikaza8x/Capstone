using MediatR;
using Microsoft.Extensions.Logging;
using Payment.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Ticketing.Application.Orders.Commands.ConfirmOrder;

namespace Ticketing.Application.Orders.EventHandlers;

public class PaymentSuccessIntegrationEventHandler(
    ISender sender,
    ILogger<PaymentSuccessIntegrationEventHandler> logger)
    : IntegrationEventHandler<PaymentSuccessIntegrationEvent>
{
    public override async Task Handle(PaymentSuccessIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        if (integrationEvent.OrderId == Guid.Empty)
            return;

        var result = await sender.Send(
           new ConfirmOrderCommand(
               integrationEvent.OrderId,
               integrationEvent.Amount,
               integrationEvent.PaidAtUtc),
           cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "PaymentSuccess handled with business failure for OrderId {OrderId}. Error: {ErrorCode} - {ErrorDescription}",
                integrationEvent.OrderId,
                result.Error.Code,
                result.Error.Description);
        }
    }
}
