using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.DeleteTicketType;

public sealed record DeleteTicketTypeCommand(Guid EventId, Guid TicketTypeId) : ICommand;