using Shared.Application.Abstractions.Messaging;
using Users.Domain.Enums;

namespace Users.Application.Features.Organizers.Commands;

public record CreateOrganizerProfileCommand(
    OrganizerType Type
) : ICommand<Guid>;