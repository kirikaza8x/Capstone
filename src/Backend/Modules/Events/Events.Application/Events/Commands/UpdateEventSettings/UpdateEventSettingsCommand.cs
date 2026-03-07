using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateEventSettings;

public sealed record UpdateEventSettingsCommand(
    Guid EventId,
    bool IsEmailReminderEnabled,
    string? UrlPath) : ICommand;
