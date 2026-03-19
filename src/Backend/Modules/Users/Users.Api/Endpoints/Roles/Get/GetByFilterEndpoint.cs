using AutoMapper;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.Application.Features.Roles.Dtos;
using Users.Application.Features.Roles.Queries;

namespace Users.Api.Roles.Get;

public class GetByFilterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/roles/search", async (
            [FromBody] RoleFilterRequestDto request,
            ISender sender,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var query = mapper.Map<GetRolesQuery>(request);

            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Roles")
        .WithName("GetRoles")
        .WithSummary("Get paged list of roles")
        .WithDescription("Returns a paged list of roles with dynamic filtering, sorting, and search options.")
        .Produces<PagedResult<RoleResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

    }
}
