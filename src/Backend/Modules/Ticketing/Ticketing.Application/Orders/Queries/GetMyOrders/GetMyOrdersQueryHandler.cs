using System.Linq;
using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Orders.Queries.GetMyOrders;

internal sealed class GetMyOrdersQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetMyOrdersQuery, PagedResult<MyOrderResponse>>
{
    public async Task<Result<PagedResult<MyOrderResponse>>> Handle(
        GetMyOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<PagedResult<MyOrderResponse>>(Error.Unauthorized(
                "GetMyOrders.Unauthorized",
                "Current user is not authenticated."));

        // Load orders
        var pagedOrders = await orderRepository.GetPagedByUserIdAsync(
            userId,
            query,
            cancellationToken);

        if (pagedOrders.Items.Count == 0)
            return Result.Success(PagedResult<MyOrderResponse>.Create(
                [],
                pagedOrders.PageNumber,
                pagedOrders.PageSize,
                pagedOrders.TotalCount));

        //  Batch fetch event summary
        var eventIds = pagedOrders.Items
            .Select(o => o.EventId)
            .Distinct()
            .ToList();

        var eventSummaryMap = await eventTicketingPublicApi
            .GetEventSummaryByEventIdsAsync(eventIds, cancellationToken);

        // build response
        var responses = pagedOrders.Items
            .Select(o =>
            {
                var eventSummary = eventSummaryMap.TryGetValue(o.EventId, out var summary)
                    ? summary
                    : null;

                return new MyOrderResponse(
                    o.Id,                   
                    o.EventId,              
                    eventSummary?.EventTitle ?? string.Empty,  
                    eventSummary?.BannerUrl,                   
                    o.Status.ToString(),    
                    o.TotalPrice,           
                    o.Tickets.Count,        
                    o.OrderVouchers.Any()   
                        ? o.OrderVouchers.Sum(ov => ov.DiscountAmount)
                        : (decimal?)null);
            })
            .ToList();

        return Result.Success(PagedResult<MyOrderResponse>.Create(
            responses,
            pagedOrders.PageNumber,
            pagedOrders.PageSize,
            pagedOrders.TotalCount));
    }
}
