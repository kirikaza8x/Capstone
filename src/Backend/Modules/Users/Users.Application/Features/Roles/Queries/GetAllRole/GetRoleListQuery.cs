using Shared.Application.Messaging;
using Users.Application.Features.Roles.Dtos;

namespace Users.Application.Features.Roles.Queries;


public record GetAllRolesQuery() : IQuery<IEnumerable<RoleResponseDto>>;
