using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.AssignAreaToTicketType;

public sealed record AssignTicketTypeAreaItem(
    Guid TicketTypeId,
    Guid AreaId);

public sealed record AssignAreaToTicketTypeCommand(
    Guid EventId,
    IReadOnlyCollection<AssignTicketTypeAreaItem> Mappings) : ICommand;
