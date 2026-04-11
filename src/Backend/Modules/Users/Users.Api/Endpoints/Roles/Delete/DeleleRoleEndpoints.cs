using AutoMapper;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Dtos;

namespace Users.Api.Roles;

public class DeleleRoleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
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
