using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UnpublishEvent;

public sealed record UnpublishEventCommand(Guid EventId) : ICommand;