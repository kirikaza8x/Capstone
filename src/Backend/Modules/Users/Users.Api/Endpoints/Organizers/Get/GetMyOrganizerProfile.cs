using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Dtos;
namespace Users.Api.Organizers;

public class GetMyOrganizerProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/organizers/me", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyOrganizerProfileQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("GetMyOrganizerProfile")
        .WithSummary("Get my organizer profile")
        .Produces<OrganizerProfileDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}