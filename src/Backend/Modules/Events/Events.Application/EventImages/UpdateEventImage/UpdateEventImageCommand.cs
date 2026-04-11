using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventImages.UpdateEventImage;

public sealed record UpdateEventImageCommand(
    Guid EventId,
    Guid ImageId,
    IFormFile File) : ICommand;
