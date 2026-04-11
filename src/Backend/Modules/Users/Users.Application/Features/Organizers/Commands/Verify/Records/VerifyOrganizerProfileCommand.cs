using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Organizers.Commands;

public record VerifyOrganizerProfileCommand(Guid UserId) : ICommand;
