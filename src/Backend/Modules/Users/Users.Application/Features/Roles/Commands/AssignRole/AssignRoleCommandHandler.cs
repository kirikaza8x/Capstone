using Roles.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Commands
{
    public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        private readonly IRoleUnitOfWork _unitOfWork;

        public AssignRoleCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IRoleUnitOfWork roleUnitOfWork
            )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = roleUnitOfWork;
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

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
