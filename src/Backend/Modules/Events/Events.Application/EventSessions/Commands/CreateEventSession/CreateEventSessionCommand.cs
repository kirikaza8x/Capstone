using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventSessions.Commands.CreateEventSession;

public sealed record CreateEventSessionItem(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime);

public sealed record CreateEventSessionCommand(
    Guid EventId,
    List<CreateEventSessionItem> Sessions) : ICommand<List<Guid>>;
