using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Commands
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.UpdateRoleRequest.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(100);

            RuleFor(x => x.UpdateRoleRequest.Description)
                .MaximumLength(500).WithMessage("Description must be at most 500 characters long.");
        }
    }

    public class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, RoleResponseDto>
    {
        private readonly IRoleRepository _repo;
        private readonly IMapper _mapper;

        public UpdateRoleCommandHandler(IRoleRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result<RoleResponseDto>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
        {
            var role = await _repo.GetByIdAsync(command.Id, cancellationToken);
            if (role == null)
            {
                return Result.Failure<RoleResponseDto>(new Error("RoleNotFound", "Role not found."));
            }

            role.Update(command.UpdateRoleRequest.Name, command.UpdateRoleRequest.Description);

            await _repo.UpdateAsync(role, cancellationToken);

            var response = _mapper.Map<RoleResponseDto>(role);

            return Result.Success(response);
        }
    }
}
