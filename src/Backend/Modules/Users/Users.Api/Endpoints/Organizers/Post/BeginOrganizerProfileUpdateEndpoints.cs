using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
namespace Users.Api.Organizers;

public class BeginOrganizerProfileUpdateEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/organizers/update/start", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new BeginOrganizerProfileUpdateCommand();

            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("BeginOrganizerProfileUpdate")
        .WithSummary("Start editing organizer profile (create draft version)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}