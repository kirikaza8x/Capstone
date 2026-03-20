using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.RequestCancelEvent;

public sealed record RequestCancelEventCommand(
    Guid EventId,
    string Reason) : ICommand;
