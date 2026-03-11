using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateTicketType;

public sealed record UpdateTicketTypeCommand(
    Guid EventId,
    Guid TicketTypeId,
    string Name,
    decimal Price) : ICommand;