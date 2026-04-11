using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;


namespace Users.Api.Organizers;

public class SubmitOrganizerProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/organizers/submit", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new SubmitOrganizerProfileCommand();

            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("SubmitOrganizerProfile")
        .WithSummary("Submit organizer profile for verification")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
