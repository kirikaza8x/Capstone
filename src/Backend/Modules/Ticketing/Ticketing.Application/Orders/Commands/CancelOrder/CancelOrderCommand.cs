using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand;
