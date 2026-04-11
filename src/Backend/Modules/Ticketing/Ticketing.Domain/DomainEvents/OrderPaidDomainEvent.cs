using Shared.Domain.DDD;

namespace Ticketing.Domain.DomainEvents;
public sealed record OrderPaidTicketItem(
    Guid OrderTicketId,
    Guid TicketTypeId,
    Guid EventSessionId,
    Guid? SeatId,
    string QrCode);

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalPrice,
    IReadOnlyList<OrderPaidTicketItem> Items) : DomainEvent;
