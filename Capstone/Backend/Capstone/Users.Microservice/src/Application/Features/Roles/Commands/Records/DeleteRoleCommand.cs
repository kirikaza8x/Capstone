using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using Users.Application.Features.Roles.Dtos;

namespace Users.Application.Features.Roles.Commands
{
    public record DeleteRoleCommand(Guid Id) : ICommand;
}
