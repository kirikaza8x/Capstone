using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Dtos;
using Microsoft.AspNetCore.Builder;

namespace Users.Api.Endpoints.Roles
{
    public class CreateRoleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/roles", async (
                RoleRequestDto request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateRoleCommand(request.Name, request.Description);
                Result<Guid> result = await sender.Send(command, cancellationToken);
                return result.ToCreated($"/api/roles/{result.Value}", "Role created successfully.");
            })
            .WithTags("Roles")
            .WithName("CreateRole")
            .WithSummary("Create a new role")
            .WithDescription("Creates a new role with the specified name and description")
            .Produces<RoleResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}
