using Shared.Application.Messaging;

namespace Order.Application.Orders.Commands.AddOrderItem;

public sealed record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity
) : ICommand;