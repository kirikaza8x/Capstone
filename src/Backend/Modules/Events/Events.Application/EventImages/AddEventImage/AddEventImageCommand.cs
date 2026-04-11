using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventImages.AddEventImage;

public sealed record AddEventImageCommand(
    Guid EventId,
    IFormFile File) : ICommand<Guid>;
