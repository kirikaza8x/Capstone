using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Roles.Dtos;
namespace Users.Application.Features.Roles.Queries.GetRoleById;
public record GetRoleByIdQuery(Guid Id) : IQuery<RoleResponseDto>;


