using AutoMapper;
using Roles.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Commands
{
    public class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, RoleResponseDto>
    {
        private readonly IRoleRepository _repo;
        private readonly IRoleUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateRoleCommandHandler(IRoleRepository repo, IRoleUnitOfWork unitOfWork, IMapper mapper)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<RoleResponseDto>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
        {
            var role = await _repo.GetByIdAsync(command.Id, cancellationToken);
            if (role == null)
            {
                return Result.Failure<RoleResponseDto>(Error.NotFound("RoleNotFound", "Role not found."));
            }

            role.Update(command.Name, command.Description);

            _repo.Update(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var response = _mapper.Map<RoleResponseDto>(role);
            return Result.Success(response);
        }
    }
}
