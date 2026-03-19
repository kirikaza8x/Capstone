using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.RejectPublishEvent;

public sealed record RejectPublishEventCommand(
    Guid EventId,
    string Reason) : ICommand;
