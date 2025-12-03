using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel.Pagination;
using Users.Application.Features.Roles.Dtos;

namespace Users.Application.Features.Roles.Queries
{
    public record GetRoleListQuery(RoleFilterDto Filter) : IQuery<PaginatedResult<RoleResponseDto>>;
}
