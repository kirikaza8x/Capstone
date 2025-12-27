using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Roles.Dtos;

namespace Users.Application.Features.Roles.Commands
{
    public record UpdateRoleCommand(Guid Id, RoleRequestDto UpdateRoleRequest) : ICommand<RoleResponseDto>,ITransactionalCommand;
}
