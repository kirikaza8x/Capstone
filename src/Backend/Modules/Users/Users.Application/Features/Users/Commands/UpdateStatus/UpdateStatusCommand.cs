using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Commands.Records
{
    public record UpdateStatusCommand(
        Guid UserId,
        UserStatus UserStatus
    ) : ICommand;
}
