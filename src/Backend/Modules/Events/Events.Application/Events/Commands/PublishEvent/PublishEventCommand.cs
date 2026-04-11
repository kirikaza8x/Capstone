using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.PublishEvent;

public sealed record PublishEventCommand(Guid EventId) : ICommand;
