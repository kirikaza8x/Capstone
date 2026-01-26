using Shared.Application.Messaging;    

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string CustomerName,
    string ShippingAddress,
    List<OrderItemRequest> Items) : ICommand<Guid>;

public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);