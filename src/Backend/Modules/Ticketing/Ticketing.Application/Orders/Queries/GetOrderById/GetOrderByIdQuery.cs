using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailResponse>;
public sealed record OrderDetailResponse(
    Guid OrderId,
    string Status,
    decimal SubTotal,
    decimal TotalPrice,
    decimal? DiscountAmount,
    DateTime? CreatedAt,

    // Event info
    Guid EventId,
    string EventTitle,
    string? BannerUrl,
    string? Location,
    DateTime? EventStartAt,

    // Tickets
    IReadOnlyList<OrderTicketResponse> Tickets);

public sealed record OrderTicketResponse(
    Guid TicketId,
    string QrCode,
    string Status,
    string TicketTypeName,
    decimal Price,
    string SessionTitle,
    DateTime SessionStartTime,
    string? SeatCode);
