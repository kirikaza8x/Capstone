using Shared.Application.Abstractions.EventBus;

namespace Ticketing.IntegrationEvents;

public sealed record OrderConfirmedTicketItem(
    Guid OrderTicketId,
    Guid TicketTypeId,
    Guid EventSessionId,
    Guid? SeatId,
    string QrCode);

public sealed record OrderConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal TotalPrice { get; init; }
    public IReadOnlyList<OrderConfirmedTicketItem> Items { get; init; }

    public OrderConfirmedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid orderId,
        Guid userId,
        decimal totalPrice,
        IReadOnlyList<OrderConfirmedTicketItem> items)
        : base(id, occurredOnUtc)
    {
        OrderId = orderId;
        UserId = userId;
        TotalPrice = totalPrice;
        Items = items;
    }
}
