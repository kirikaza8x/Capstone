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

public sealed record OrderListMeta(
    int TotalOrders,
    decimal TotalRevenue,
    decimal TotalDiscount,
    int CancelledOrders
);

public sealed record GetAllOrdersMetaResponse(
    OrderListMeta Meta,
    IReadOnlyList<OrderListItemResponse> Data
);
