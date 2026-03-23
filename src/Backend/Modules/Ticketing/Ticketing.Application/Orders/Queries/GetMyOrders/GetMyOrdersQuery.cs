using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Ticketing.Application.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery : PagedQuery, IQuery<PagedResult<MyOrderResponse>>;

public sealed record MyOrderResponse(
    Guid OrderId,           
    Guid EventId,           
    string EventTitle,      
    string? BannerUrl,      
    string Status,          
    decimal TotalPrice,     
    int TotalTickets,       
    decimal? DiscountAmount);
