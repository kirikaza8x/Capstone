using Shared.Application.Messaging;

namespace Events.Application.EventSessions.Commands.CreateEventSession;

public sealed record CreateEventSessionCommand(
    Guid EventId,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime
) : ICommand<Guid>;
