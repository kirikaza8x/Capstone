using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Ticketing.Application.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(
    Guid EventId,
    string? Status
) : PagedQuery, IQuery<GetAllOrdersResponse>;

public sealed record GetAllOrdersResponse(
    PagedResult<OrderListItemResponse> Orders,
    OrderOverviewResponse Summary
);

public sealed record OrderOverviewResponse(
    int TotalOrders,
    decimal GrossRevenue,
    decimal NetRevenue,
    decimal TotalDiscount,
    int CancelledOrders
);

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
