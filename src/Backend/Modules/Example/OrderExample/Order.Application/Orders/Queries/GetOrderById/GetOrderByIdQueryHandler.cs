using Order.Domain.Orders;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Order.Application.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDetailResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDetailResponse>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound(request.OrderId));

        var response = new OrderDetailResponse(
            order.Id,
            order.CustomerId,
            order.CustomerName,
            order.ShippingAddress,
            order.Status.ToString(),
            order.TotalAmount,
            order.OrderItems.Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice)).ToList(),
            order.CreatedAt);

        return Result.Success(response);
    }
}