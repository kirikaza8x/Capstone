using Shared.Application.Messaging;

namespace Order.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailResponse>;

public sealed record OrderDetailResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string ShippingAddress,
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items,
    DateTime? CreatedAt);

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);