using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Roles.Commands
{
    public record DeleteRoleCommand(Guid Id) : ICommand, ITransactionalCommand;
}
