using Shared.Application.Messaging;

namespace Events.Application.Events.Commands.UpdateEventSettings;

public sealed record UpdateEventSettingsCommand(
    Guid EventId,
    bool IsEmailReminderEnabled,
    string? UrlPath) : ICommand;
