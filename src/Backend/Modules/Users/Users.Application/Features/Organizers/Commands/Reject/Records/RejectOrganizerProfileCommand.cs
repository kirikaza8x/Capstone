using Shared.Application.Abstractions.Messaging;

public record RejectOrganizerProfileCommand(
    Guid UserId,
    string Reason) : ICommand;
