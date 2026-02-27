using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Roles.Commands;
using Microsoft.AspNetCore.Builder;

namespace Users.Api.Endpoints.Roles
{
    public class AssignRoleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Assign Role
            app.MapPost("api/users/{userId}/roles/{roleId}", async (
                Guid userId,
                Guid roleId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new AssignRoleCommand(userId, roleId);

                Result result = await sender.Send(command, cancellationToken);

                return result.ToOk("Role assigned successfully.");
            })
            .WithTags("Roles")
            .WithName("AssignRole")
            .WithSummary("Assign a role to a user")
            .WithDescription("Assigns the specified role to the specified user")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}
