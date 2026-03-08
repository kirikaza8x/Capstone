using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.CancelEvent;

public sealed record CancelEventCommand(
    Guid EventId,
    string? Reason) : ICommand;