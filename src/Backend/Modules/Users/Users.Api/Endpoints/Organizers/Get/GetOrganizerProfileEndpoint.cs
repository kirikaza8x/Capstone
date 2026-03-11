using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries.GetOrganizerProfile;

namespace Users.Api.Endpoints.Organizers.Get;

public class GetOrganizerProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/organizers/profile", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrganizerProfileQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("GetOrganizerProfile")
        .WithSummary("Get organizer profile")
        .WithDescription("Retrieve the organizer profile of the current user.")
        .Produces<OrganizerProfileResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }
}