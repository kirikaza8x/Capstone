using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.DeleteEvent;

public sealed record DeleteEventCommand(Guid EventId) : ICommand;
