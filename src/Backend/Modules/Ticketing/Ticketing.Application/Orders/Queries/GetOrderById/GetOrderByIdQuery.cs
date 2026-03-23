using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<IReadOnlyList<OrderTicketResponse>>;

public sealed record OrderTicketResponse(
    Guid TicketId,
    string QrCode,
    string Status,
    string TicketTypeName,
    decimal Price,
    string SessionTitle,
    DateTime SessionStartTime,
    string? SeatCode);
