

using Shared.Application.Messaging;

namespace Order.Application.Orders.Commands.ConfirmOrder;

public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand;
