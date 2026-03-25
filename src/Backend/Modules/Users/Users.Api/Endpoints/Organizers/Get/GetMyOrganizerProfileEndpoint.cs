using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Queries;
public class GetMyOrganizerProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizer/me", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyOrganizerProfileQuery();

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetMyOrganizerProfile")
        .WithTags("Organizers");
    }
}