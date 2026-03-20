using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Dtos;
namespace Users.Api.Organizers;

public class GetOrganizerPublicProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/organizers/{userId:guid}", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerPublicProfileQuery(userId);

            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("GetOrganizerPublicProfile")
        .WithSummary("Get public organizer profile")
        .Produces<OrganizerPublicProfileDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
