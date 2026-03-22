namespace Ticketing.PublicApi;

public interface ITicketingPublicApi
{
    // Called at payment initiation — gets ticket breakdown for an order
    // Returns null if order not found or does not belong to userId
    Task<OrderDetails?> GetOrderAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

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
