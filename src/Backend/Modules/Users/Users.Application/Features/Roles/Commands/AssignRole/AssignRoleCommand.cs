
using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Roles.Commands
{
    public record AssignRoleCommand(Guid UserId, Guid RoleId) : ICommand;
}
