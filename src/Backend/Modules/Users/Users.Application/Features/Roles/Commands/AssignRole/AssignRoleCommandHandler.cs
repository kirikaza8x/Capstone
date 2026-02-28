using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Commands
{
    public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public AssignRoleCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<Result> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<Result>(Error.NotFound("UserNotFound", "User does not exist."));
            }

            var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
            if (role == null)
            {
                return Result.Failure<Result>(Error.NotFound("RoleNotFound", "Role does not exist."));
            }

            user.AssignRole(role);

            _userRepository.Update(user);

            return Result.Success();
        }
    }
}
