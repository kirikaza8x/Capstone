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
) : IQueryHandler<GetAllOrdersQuery, PagedResult<OrderListItemResponse>>
{
    public async Task<Result<PagedResult<OrderListItemResponse>>> Handle(
        GetAllOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        // Get event summary to check if event exists and if current user is the organizer
        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            new[] { query.EventId }, cancellationToken);

        var eventSummary = eventSummaryMap.TryGetValue(query.EventId, out var summary) ? summary : null;
        if (eventSummary == null)
            return Result.Failure<PagedResult<OrderListItemResponse>>(Error.NotFound(
                "GetAllOrders.EventNotFound",
                "Event not found."));

        if (eventSummary.OrganizerId != userId)
            return Result.Failure<PagedResult<OrderListItemResponse>>(Error.Forbidden(
                "GetAllOrders.Forbidden",
                "You are not the organizer of this event."));

        // get paged orders for the event
        var pagedOrders = await orderRepository.GetPagedByEventIdAsync(
            query.EventId, query.Status, query, cancellationToken);

        if (pagedOrders.Items.Count == 0)
            return Result.Success(PagedResult<OrderListItemResponse>.Create(
                [],
                pagedOrders.PageNumber,
                pagedOrders.PageSize,
                pagedOrders.TotalCount));

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
            decimal? discountAmount = null;
            if (voucher != null)
            {
                if (voucher.Type == VoucherType.Percentage)
                    discountAmount = Math.Round(originalTotalPrice * voucher.Value / 100, 0, MidpointRounding.AwayFromZero);
                else if (voucher.Type == VoucherType.Fixed)
                    discountAmount = Math.Min(voucher.Value, originalTotalPrice);
            }

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

        return Result.Success(PagedResult<OrderListItemResponse>.Create(
            responses,
            pagedOrders.PageNumber,
            pagedOrders.PageSize,
            pagedOrders.TotalCount));
    }
}
