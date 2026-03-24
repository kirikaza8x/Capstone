using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;

namespace Users.Api.Organizers;

public class GetOrganizerProfileDetailEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/organizers/detail/{id}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerPublicProfileQuery(id);

            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("GetOrganizerProfileDetail")
        .WithSummary("Get organizer profile detail by ID")
        .WithDescription("Fetches detailed information about a specific organizer profile using its unique identifier.")
        .Produces<OrganizerProfileDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
