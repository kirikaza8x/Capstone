using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Commands.ConfirmOrder;

public sealed record ConfirmOrderCommand(
    Guid OrderId,
    decimal Amount,
    DateTime PaidAtUtc) : ICommand;