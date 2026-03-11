using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.AssignAreaToTicketType;

public sealed record AssignAreaToTicketTypeCommand(
    Guid EventId,
    Guid TicketTypeId,
    Guid AreaId) : ICommand;