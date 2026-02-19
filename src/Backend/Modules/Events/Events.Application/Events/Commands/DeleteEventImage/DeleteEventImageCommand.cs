using Shared.Application.Messaging;

namespace Events.Application.Events.Commands.DeleteEventImage;

public sealed record DeleteEventImageCommand(
    Guid EventId,
    Guid ImageId) : ICommand;