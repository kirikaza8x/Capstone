using Carter;
using Events.Application.Events.Commands.RequestPublishEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public class RequestPublishEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.OrganizerEventById}/request-publish", async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new RequestPublishEventCommand(eventId),
                cancellationToken);

            return result.ToOk("Event submitted for review successfully.");
        })
        .WithTags(Constants.Tags.EventForOrganizer)
        .WithName("RequestPublishEvent")
        .WithSummary("Request event publish")
        .WithDescription("Organizer submits a draft event for admin review.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
