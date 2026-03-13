using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateTicketType;

public sealed record UpdateTicketTypeCommand(
    Guid EventId,
    Guid TicketTypeId,
    string Name,
    int Quantity,
    decimal Price) : ICommand;