using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetOrderByIdQuery, OrderDetailResponse>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<OrderDetailResponse>(Error.Unauthorized(
                "GetOrderById.Unauthorized",
                "Current user is not authenticated."));

        var order = await orderRepository.GetByIdWithOrderTicketAsync(
            query.OrderId,
            cancellationToken);

        if (order is null)
            return Result.Failure<OrderDetailResponse>(
                TicketingErrors.Order.NotFound(query.OrderId));

        if (order.UserId != userId)
            return Result.Failure<OrderDetailResponse>(Error.Forbidden(
                "GetOrderById.Forbidden",
                "You are not allowed to view this order."));

        var ticketItems = order.Tickets
            .Select(t => (t.TicketTypeId, t.EventSessionId, t.SeatId))
            .ToList();

        var ticketDetailMap = await eventTicketingPublicApi
            .GetOrderTicketDetailsAsync(ticketItems, cancellationToken);

        var ticketResponses = order.Tickets.Select(t =>
        {
            var detail = ticketDetailMap.TryGetValue((t.TicketTypeId, t.EventSessionId), out var d) ? d : null;

            return new OrderTicketResponse(
                t.Id,
                t.QrCode,
                t.Status.ToString(),
                detail?.TicketTypeName ?? string.Empty,
                detail?.Price ?? 0m,
                detail?.SessionTitle ?? string.Empty,
                detail?.SessionStartTime ?? DateTime.MinValue,
                detail?.SeatCode);
        }).ToList();

        // get event summary for event info
        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            new[] { order.EventId }, cancellationToken);

        var eventSummary = eventSummaryMap.TryGetValue(order.EventId, out var summary) ? summary : null;
        var eventId = eventSummary?.EventId ?? Guid.Empty;
        var eventTitle = eventSummary?.EventTitle ?? string.Empty;
        var bannerUrl = eventSummary?.BannerUrl;
        var location = eventSummary?.Location;
        var eventStartAt = eventSummary?.EventStartAt;

        var subTotal = ticketResponses.Sum(t => t.Price);
        var discountAmount = order.OrderVouchers.FirstOrDefault()?.DiscountAmount ?? 0m;
        var totalPrice = subTotal - discountAmount;

        var response = new OrderDetailResponse(
            order.Id,
            order.Status.ToString(),
            subTotal,
            totalPrice,
            discountAmount,
            order.CreatedAt,
            eventId,
            eventTitle,
            bannerUrl,
            location,
            eventStartAt,
            ticketResponses);

        return Result.Success(response);
    }
}
