using Shared.Application.Messaging;
using Users.Application.Features.Roles.Dtos;

namespace Users.Application.Features.Roles.Commands
{

    public record CreateRoleCommand(string Name, string? Description ="") : ICommand<Guid>;

}
