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
    : IQueryHandler<GetOrderByIdQuery, IReadOnlyList<OrderTicketResponse>>
{
    public async Task<Result<IReadOnlyList<OrderTicketResponse>>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<IReadOnlyList<OrderTicketResponse>>(Error.Unauthorized(
                "GetOrderById.Unauthorized",
                "Current user is not authenticated."));

        var order = await orderRepository.GetByIdWithOrderTicketAsync(
            query.OrderId,
            cancellationToken);

        if (order is null)
            return Result.Failure<IReadOnlyList<OrderTicketResponse>>(
                TicketingErrors.Order.NotFound(query.OrderId));

        if (order.UserId != userId)
            return Result.Failure<IReadOnlyList<OrderTicketResponse>>(Error.Forbidden(
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

        return Result.Success<IReadOnlyList<OrderTicketResponse>>(ticketResponses);
    }
}
