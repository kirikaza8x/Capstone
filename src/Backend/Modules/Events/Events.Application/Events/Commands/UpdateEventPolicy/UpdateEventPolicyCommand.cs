using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateEventPolicy;

public sealed record UpdateEventPolicyCommand(
    Guid EventId,
    string Policy) : ICommand;
