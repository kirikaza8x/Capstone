using AutoMapper;
using Roles.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Commands
{


    public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
    {
        private readonly IRoleRepository _repo;

        private IRoleUnitOfWork _unitOfWork;

        public DeleteRoleCommandHandler(IRoleRepository repo, IRoleUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
        {
            var role = await _repo.GetByIdAsync(command.Id, cancellationToken);
            if (role == null)
            {
                return Result.Failure(Error.NotFound("RoleNotFound", "Role not found."));
            }

            _repo.Remove(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
