using MediatR;
using Shared.Application.Abstractions.EventBus;
using Ticketing.Domain.DomainEvents;
using Ticketing.IntegrationEvents;

namespace Ticketing.Application.Orders.EventHandlers;

internal sealed class OrderPaidDomainEventHandler(
    IEventBus eventBus) : INotificationHandler<OrderPaidDomainEvent>
{
    public async Task Handle(
        OrderPaidDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(
            new OrderConfirmedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                notification.OrderId,
                notification.UserId,
                notification.TotalPrice,
                notification.Items
                    .Select(i => new OrderConfirmedTicketItem(
                        i.OrderTicketId,
                        i.TicketTypeId,
                        i.EventSessionId,
                        i.SeatId,
                        i.QrCode))
                    .ToList()),
            cancellationToken);
    }
}
