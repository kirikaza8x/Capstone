using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.DeleteEventImage;

public sealed record DeleteEventImageCommand(
    Guid EventId,
    Guid ImageId) : ICommand;