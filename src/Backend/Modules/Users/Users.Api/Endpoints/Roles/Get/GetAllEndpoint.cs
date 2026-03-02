using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Roles.Dtos;
using Users.Application.Features.Roles.Queries;
using Carter;

namespace Users.Api.Roles.Get;

public class GetRolesEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/roles", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllRolesQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Roles")
        .WithName("GetAllRoles")
        .WithSummary("Get all roles")
        .WithDescription("Returns all roles without paging.")
        .Produces<IEnumerable<RoleResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
