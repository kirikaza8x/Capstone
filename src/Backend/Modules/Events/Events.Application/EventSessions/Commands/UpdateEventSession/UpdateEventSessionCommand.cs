using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventSessions.Commands.UpdateEventSession;

public sealed record UpdateEventSessionCommand(
    Guid EventId,
    Guid SessionId,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime) : ICommand;
