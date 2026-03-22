using MediatR;
using Microsoft.Extensions.Logging;
using Notifications.Application.Commands.SendOrderConfirmationEmail;
using Shared.Application.Abstractions.EventBus;
using Ticketing.IntegrationEvents;

namespace Notifications.Application.IntegrationEventHandlers;

public class SendOrderConfirmationEmailIntegrationEventHandler(
    ISender sender,
    ILogger<SendOrderConfirmationEmailIntegrationEventHandler> logger)
    : IntegrationEventHandler<OrderConfirmedIntegrationEvent>
{
    public override async Task Handle(
        OrderConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (integrationEvent.UserId == Guid.Empty)
            return;

        var result = await sender.Send(
            new SendOrderConfirmationEmailCommand(
                integrationEvent.UserId,
                integrationEvent.OrderId,
                integrationEvent.TotalPrice,
                integrationEvent.OccurredOnUtc,
                integrationEvent.Items),
            cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "SendOrderConfirmationEmail handled with business failure for OrderId {OrderId}. Error: {ErrorCode} - {ErrorDescription}",
                integrationEvent.OrderId,
                result.Error.Code,
                result.Error.Description);
        }
    }
}
