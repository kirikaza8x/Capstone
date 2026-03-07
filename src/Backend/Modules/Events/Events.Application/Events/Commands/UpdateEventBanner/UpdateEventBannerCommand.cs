using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateEventBanner;

public sealed record UpdateEventBannerCommand(
    Guid EventId,
    string BannerUrl) : ICommand;