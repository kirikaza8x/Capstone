using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.RequestPublishEvent;

public sealed record RequestPublishEventCommand(Guid EventId) : ICommand;