using Events.Domain.Enums;
using Shared.Application.Messaging;

namespace Events.Application.Events.Commands.CreateTicketType;

public sealed record CreateTicketTypeCommand(
    Guid EventSessionId,
    string Name,
    decimal Price,
    int Quantity,
    AreaType Type,
    Guid? AreaId
) : ICommand<Guid>;
