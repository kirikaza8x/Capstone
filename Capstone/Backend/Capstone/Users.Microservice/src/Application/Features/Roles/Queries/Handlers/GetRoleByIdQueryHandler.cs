using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Queries
{

    public class GetRoleByIdQueryHandler 
        : IQueryHandler<GetRoleByIdQuery, RoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public GetRoleByIdQueryHandler(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<Result<RoleResponseDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (role is null)
                return Result.Failure<RoleResponseDto>(new Error("RoleNotFound", "Role not found"));

            return Result.Success(_mapper.Map<RoleResponseDto>(role));
        }
    }
}
