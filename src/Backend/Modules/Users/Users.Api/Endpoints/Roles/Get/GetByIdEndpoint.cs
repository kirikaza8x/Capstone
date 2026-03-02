using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Roles.Dtos;
using Carter;
using Users.Application.Features.Roles.Queries.GetRoleById;

namespace Users.Api.Roles.Get;

public class GetByIdRolesEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/roles/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRoleByIdQuery(id);

            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Roles")
        .WithName("GetRoleById")
        .WithSummary("Get role by Id")
        .WithDescription("Returns a role by its unique identifier.")
        .Produces<RoleResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

    }
}
