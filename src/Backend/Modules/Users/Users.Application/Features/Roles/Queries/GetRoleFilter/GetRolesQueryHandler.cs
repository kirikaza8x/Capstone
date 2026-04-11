using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles.Queries;


public sealed class GetRolesQueryHandler
    : IQueryHandler<GetRolesQuery, PagedResult<RoleResponseDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public GetRolesQueryHandler(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<RoleResponseDto>>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        var pagedResult = await _roleRepository.GetPagedAsync(
            query,
            selector: r => _mapper.Map<RoleResponseDto>(r),
            predicate: r =>
                (string.IsNullOrWhiteSpace(query.Name) || r.Name.Contains(query.Name)) &&
                (string.IsNullOrWhiteSpace(query.Description) || (r.Description != null && r.Description.Contains(query.Description))) &&
                (string.IsNullOrWhiteSpace(query.SearchTerm) ||
                    (r.Name != null && r.Name.Contains(query.SearchTerm)) ||
                    (r.Description != null && r.Description.Contains(query.SearchTerm))),
            cancellationToken: cancellationToken);

        return Result.Success(pagedResult);
    }
}

