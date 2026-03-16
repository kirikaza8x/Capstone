using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.RejectCancelEvent;

public sealed record RejectCancelEventCommand(
    Guid EventId,
    string Reason) : ICommand;