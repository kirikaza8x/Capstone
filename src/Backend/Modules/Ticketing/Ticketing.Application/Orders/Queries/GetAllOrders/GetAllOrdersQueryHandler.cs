using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;
using Users.PublicApi.PublicApi;

namespace Ticketing.Application.Orders.Queries.GetAllOrders;

internal sealed class GetAllOrdersQueryHandler(
    IOrderRepository orderRepository,
    IVoucherRepository voucherRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    IUserPublicApi userPublicApi,
    ICurrentUserService currentUserService
) : IQueryHandler<GetAllOrdersQuery, GetAllOrdersResponse>
{
    public async Task<Result<GetAllOrdersResponse>> Handle(
        GetAllOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        // Get event summary to check if event exists and if current user is the organizer
        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            new[] { query.EventId }, cancellationToken);

        var eventSummary = eventSummaryMap.TryGetValue(query.EventId, out var summary) ? summary : null;
        if (eventSummary == null)
            return Result.Failure<GetAllOrdersResponse>(Error.NotFound(
                "GetAllOrders.EventNotFound",
                "Event not found."));

        if (eventSummary.OrganizerId != userId)
            return Result.Failure<GetAllOrdersResponse>(Error.Forbidden(
                "GetAllOrders.Forbidden",
                "You are not the organizer of this event."));

        var allOrders = await orderRepository.GetAllByEventIdAsync(query.EventId, cancellationToken);
        var paidOrders = allOrders.Where(o => o.Status == OrderStatus.Paid).ToList();
        var totalDiscount = paidOrders.SelectMany(o => o.OrderVouchers).Sum(v => v.DiscountAmount);
        var netRevenue = paidOrders.Sum(o => o.TotalPrice);
        var grossRevenue = netRevenue + totalDiscount;

        var summaryResponse = new OrderOverviewResponse(
            TotalOrders: allOrders.Count,
            GrossRevenue: grossRevenue,
            NetRevenue: netRevenue,
            TotalDiscount: totalDiscount,
            CancelledOrders: allOrders.Count(o => o.Status == OrderStatus.Cancelled));

        // get paged orders for the event
        var pagedOrders = await orderRepository.GetPagedByEventIdAsync(
            query.EventId, query.Status, query, cancellationToken);

        // get user info for buyers
        var userIds = pagedOrders.Items.Select(o => o.UserId).Distinct().ToList();
        var userMap = await userPublicApi.GetUserMapByIdsAsync(userIds, cancellationToken);

        var voucherIds = pagedOrders.Items
            .SelectMany(o => o.OrderVouchers.Select(v => v.VoucherId))
            .Distinct()
            .ToList();
        var voucherMap = await voucherRepository.GetVoucherMapByIdsAsync(voucherIds, cancellationToken);


        // Build response
        var responses = pagedOrders.Items.Select(order =>
        {
            var user = userMap.TryGetValue(order.UserId, out var u) ? u : null;
            var orderVoucher = order.OrderVouchers.FirstOrDefault();
            Voucher? voucher = null;
            if (orderVoucher != null)
                voucherMap.TryGetValue(orderVoucher.VoucherId, out voucher);

            var originalTotalPrice = order.OriginalTotalPrice;
            decimal? discountAmount = order.OrderVouchers.Count > 0
                ? order.OrderVouchers.Sum(v => v.DiscountAmount)
                : null;

            return new OrderListItemResponse(
                OrderId: order.Id,
                CreatedAt: order.CreatedAt,
                BuyerName: user?.FullName ?? "",
                BuyerEmail: user?.Email ?? "",
                TotalPrice: order.TotalPrice,
                OriginalPrice: originalTotalPrice,
                VoucherCode: voucher?.CouponCode,
                DiscountAmount: discountAmount,
                Status: order.Status.ToString()
            );
        }).ToList();

        var pagedResult = PagedResult<OrderListItemResponse>.Create(
            responses,
            pagedOrders.PageNumber,
            pagedOrders.PageSize,
            pagedOrders.TotalCount);

        return Result.Success(new GetAllOrdersResponse(
            Orders: pagedResult,
            Summary: summaryResponse));
    }
}
