using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderTicketItem(
    Guid EventSessionId,
    Guid TicketTypeId,
    Guid? SeatId);

public sealed record CreateOrderCommand(
    List<CreateOrderTicketItem> Tickets) : ICommand<Guid>;
