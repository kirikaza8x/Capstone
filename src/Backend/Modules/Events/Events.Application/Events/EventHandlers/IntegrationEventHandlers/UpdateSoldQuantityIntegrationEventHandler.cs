using Events.Application.Events.Commands.UpdateSoldQuantity;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Ticketing.IntegrationEvents;

namespace Events.Application.Events.EventHandlers.IntegrationEventHandlers;
public class UpdateSoldQuantityIntegrationEventHandler(
    ISender sender,
    ILogger<UpdateSoldQuantityIntegrationEventHandler> logger)
    : IntegrationEventHandler<OrderConfirmedIntegrationEvent>
{
    public override async Task Handle(
            OrderConfirmedIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
    {
        if (integrationEvent.OrderId == Guid.Empty)
            return;

        var items = integrationEvent.Items
            .GroupBy(i => i.TicketTypeId)
            .Select(g => new UpdateSoldQuantityItem(g.Key, g.Count()))
            .ToList();

        var result = await sender.Send(
            new UpdateSoldQuantityCommand(items),
            cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "UpdateSoldQuantity handled with business failure for OrderId {OrderId}. Error: {ErrorCode} - {ErrorDescription}",
                integrationEvent.OrderId,
                result.Error.Code,
                result.Error.Description);
        }
    }
}
