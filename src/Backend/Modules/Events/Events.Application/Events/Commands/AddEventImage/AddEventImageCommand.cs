using Microsoft.AspNetCore.Http;
using Shared.Application.Messaging;

namespace Events.Application.Events.Commands.AddEventImage;

public sealed record AddEventImageCommand(
    Guid EventId,
    IFormFile File) : ICommand<Guid>;
