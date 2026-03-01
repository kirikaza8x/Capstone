using Shared.Application.Messaging;

namespace Users.Application.Features.Roles.Commands
{

    public record CreateRoleCommand(string Name, string? Description = "") : ICommand<Guid>;

}
