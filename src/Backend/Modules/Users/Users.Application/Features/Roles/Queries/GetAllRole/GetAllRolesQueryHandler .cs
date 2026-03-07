using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Queries
{

    public class GetAllRolesQueryHandler
        : IQueryHandler<GetAllRolesQuery, IEnumerable<RoleResponseDto>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public GetAllRolesQueryHandler(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<RoleResponseDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetAllAsync(cancellationToken);

            var dtos = roles.Select(r => _mapper.Map<RoleResponseDto>(r));
            return Result.Success(dtos);
        }
    }



}
