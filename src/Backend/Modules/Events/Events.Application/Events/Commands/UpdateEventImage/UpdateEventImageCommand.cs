using Microsoft.AspNetCore.Http;
using Shared.Application.Messaging;

namespace Events.Application.Events.Commands.UpdateEventImage;

public sealed record UpdateEventImageCommand(
    Guid EventId,
    Guid ImageId,
    IFormFile File) : ICommand;