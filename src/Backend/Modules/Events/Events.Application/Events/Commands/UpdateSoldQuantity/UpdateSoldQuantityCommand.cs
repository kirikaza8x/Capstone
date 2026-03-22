using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateSoldQuantity;

public sealed record UpdateSoldQuantityItem(
    Guid TicketTypeId,
    int Quantity);

public sealed record UpdateSoldQuantityCommand(
    IReadOnlyList<UpdateSoldQuantityItem> Items) : ICommand;
