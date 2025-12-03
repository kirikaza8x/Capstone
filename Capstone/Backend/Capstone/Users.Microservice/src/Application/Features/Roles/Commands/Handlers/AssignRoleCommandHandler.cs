using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
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
            // 1. Get user
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<Result>(new Error("UserNotFound", "User does not exist."));
            }

            // 2. Get role
            var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
            if (role == null)
            {
                return Result.Failure<Result>(new Error("RoleNotFound", "Role does not exist."));
            }

            // 3. Assign role (domain logic)
            user.AssignRole(role);

            // 4. Persist changes
            await _userRepository.UpdateAsync(user, cancellationToken);

            // 5. Map response
            return Result.Success();
        }
    }
}
