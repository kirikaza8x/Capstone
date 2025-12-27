using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Commands.Login
{

    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.CreateRoleRequest.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(100);

            RuleFor(x => x.CreateRoleRequest.Description)
                .MaximumLength(500).WithMessage("Description must be at most 500 characters long.");
        }
    }
    public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, RoleResponseDto>
    {
        private readonly IRoleRepository _repo;
        private readonly IMapper _mapper;


        public CreateRoleCommandHandler(
            IRoleRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result<RoleResponseDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
        {
            
            var existingRole = await _repo.GetByRoleNameAsync(command.CreateRoleRequest.Name, cancellationToken);
            if (existingRole != null)
            {
                return Result.Failure<RoleResponseDto>(new Error("RoleAlreadyExists", "A role with the same name already exists."));
            }

            Role role = Role.Create(
                command.CreateRoleRequest.Name,
                command.CreateRoleRequest.Description
            );
            await _repo.AddAsync(role, cancellationToken);
            var response = _mapper.Map<RoleResponseDto>(role);
            
            return Result.Success(response);
        }
    }

}