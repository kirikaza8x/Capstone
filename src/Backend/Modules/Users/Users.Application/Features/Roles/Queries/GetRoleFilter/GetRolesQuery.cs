using Shared.Application.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Enums;

namespace Users.Application.Features.Roles.Queries;

public sealed record GetRolesQuery : AdvancedPagedQuery, IQuery<PagedResult<RoleResponseDto>>
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}
