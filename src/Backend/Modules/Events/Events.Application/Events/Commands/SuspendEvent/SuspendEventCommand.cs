using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.SuspendEvent;

public sealed record SuspendEventCommand(
    Guid EventId,
    string Reason,
    int FixWindowHours) : ICommand;
