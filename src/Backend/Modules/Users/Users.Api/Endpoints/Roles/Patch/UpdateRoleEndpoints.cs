using MediatR;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Dtos;
using Carter;

namespace Users.Api.Roles;

public class RoleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Update Role Endpoint
        app.MapPut("api/roles/{id}", async (
            Guid id,
            [FromBody] RoleRequestDto request,
            ISender sender,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var command = mapper.Map<UpdateRoleCommand>(request);
            command = command with { Id = id };

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Roles")
        .WithName("UpdateRole")
        .WithSummary("Update an existing role")
        .WithDescription("Updates the name and description of a role by its ID.")
        .Produces<RoleResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Delete Role Endpoint
        app.MapDelete("api/roles/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteRoleCommand(id);

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Roles")
        .WithName("DeleteRole")
        .WithSummary("Delete a role")
        .WithDescription("Deletes a role by its ID.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
