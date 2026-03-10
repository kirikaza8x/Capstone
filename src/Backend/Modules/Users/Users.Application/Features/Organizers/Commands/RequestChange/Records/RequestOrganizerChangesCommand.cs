using Shared.Application.Abstractions.Messaging;

public record RequestOrganizerChangesCommand(Guid UserId) : ICommand;