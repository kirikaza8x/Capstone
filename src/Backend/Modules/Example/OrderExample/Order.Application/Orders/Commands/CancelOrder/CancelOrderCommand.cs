using Shared.Application.Messaging;

namespace Order.Application.Orders.Commands.CancelOrder;


public sealed record CancelOrderCommand(Guid OrderId) : ICommand;