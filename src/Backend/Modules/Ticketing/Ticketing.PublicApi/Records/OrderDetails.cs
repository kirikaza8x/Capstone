namespace Ticketing.PublicApi.Records;

public record OrderDetails(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    IReadOnlyList<OrderTicketDetail> Tickets
);

public record OrderTicketDetail(
    Guid OrderTicketId,
    Guid EventSessionId,
    decimal Amount
);
