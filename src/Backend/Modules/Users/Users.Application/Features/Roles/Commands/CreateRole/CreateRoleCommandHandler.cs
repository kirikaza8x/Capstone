using AutoMapper;
using Roles.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
    {
        private readonly IRoleRepository _repo;
        private readonly IRoleUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public CreateRoleCommandHandler(
            IRoleRepository repo,
            IRoleUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
        {
            var existingRole = await _repo.GetByRoleNameAsync(command.Name, cancellationToken);
            if (existingRole != null)
            {
                return Result.Failure<Guid>(Error.Conflict("RoleAlreadyExists", "A role with the same name already exists."));
            }

            Role role = Role.Create(
                command.Name,
                command.Description
            );
            _repo.Add(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(role.Id);
        }
    }

}
