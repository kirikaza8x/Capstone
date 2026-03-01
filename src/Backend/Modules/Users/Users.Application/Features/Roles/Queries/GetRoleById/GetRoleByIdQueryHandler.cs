using AutoMapper;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Roles.Dtos;
using Users.Application.Features.Roles.Queries.GetRoleById;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Queries
{    public class GetRoleByIdQueryHandler 
        : IQueryHandler<GetRoleByIdQuery, RoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public GetRoleByIdQueryHandler(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<Result<RoleResponseDto>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(query.Id, cancellationToken);

            if (role is null)
            {
                return Result.Failure<RoleResponseDto>(
                    Error.NotFound("Role.NotFound", "Role not found.")
                );
            }

            var dto = _mapper.Map<RoleResponseDto>(role);
            return Result.Success(dto);
        }
    }

}
