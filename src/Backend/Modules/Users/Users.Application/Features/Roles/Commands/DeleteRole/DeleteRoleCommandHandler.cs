// using AutoMapper;
// using FluentValidation;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Application.Common.ResponseModel;
// using Users.Application.Features.Roles.Dtos;
// using Users.Domain.Repositories;

// namespace Users.Application.Features.Roles.Commands
// {
//     public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
//     {
//         public DeleteRoleCommandValidator()
//         {
//             RuleFor(x => x.Id)
//                 .NotEmpty().WithMessage("Role Id is required.");
//         }
//     }

//     public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
//     {
//         private readonly IRoleRepository _repo;
//         private readonly IMapper _mapper;

//         public DeleteRoleCommandHandler(IRoleRepository repo, IMapper mapper)
//         {
//             _repo = repo;
//             _mapper = mapper;
//         }

//         public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
//         {
//             var role = await _repo.GetByIdAsync(command.Id, cancellationToken);
//             if (role == null)
//             {
//                 return Result.Failure<RoleResponseDto>(new Error("RoleNotFound", "Role not found."));
//             }

//             await _repo.DeleteAsync(role, cancellationToken);
//             return Result.Success();
//         }
//     }
// }
