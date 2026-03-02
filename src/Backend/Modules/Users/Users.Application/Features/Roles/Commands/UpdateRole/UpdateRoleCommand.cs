using Shared.Application.Messaging;
using Users.Application.Features.Roles.Dtos;

namespace Users.Application.Features.Roles.Commands
{
    public record UpdateRoleCommand(Guid Id, string Name, string? Description = "") : ICommand<RoleResponseDto>;

}
