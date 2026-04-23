using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Ticketing.Application.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(
    Guid EventId,
    string? Status
) : PagedQuery, IQuery<PagedResult<OrderListItemResponse>>;

public sealed record OrderListItemResponse(
    Guid OrderId,
    DateTime? CreatedAt,
    string BuyerName,
    string BuyerEmail,
    decimal TotalPrice,
    decimal? OriginalPrice,
    string? VoucherCode,
    decimal? DiscountAmount,
    string Status
);
