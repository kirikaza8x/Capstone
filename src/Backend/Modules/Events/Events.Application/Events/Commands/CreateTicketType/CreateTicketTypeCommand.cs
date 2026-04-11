using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.CreateTicketType;

public sealed record CreateTicketTypeCommand(
    Guid EventId,
    string Name,
    int Quantity,
    decimal Price) : ICommand<Guid>;
