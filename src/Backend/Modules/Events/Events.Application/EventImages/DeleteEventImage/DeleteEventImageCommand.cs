using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventImages.DeleteEventImage;

public sealed record DeleteEventImageCommand(
    Guid EventId,
    Guid ImageId) : ICommand;
