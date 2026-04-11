using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventSessions.Commands.DeleteEventSession;

public sealed record DeleteEventSessionCommand(Guid EventId, Guid SessionId) : ICommand;
