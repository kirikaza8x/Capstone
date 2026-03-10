using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateTicketType;

public sealed record UpdateTicketTypeCommand(
    Guid SessionId,
    Guid TicketTypeId,
    string Name,
    decimal Price,
    int Quantity) : ICommand;