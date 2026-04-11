using Carter;
using Events.Application.Events.Commands.UpdateEventBanner;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public class UpdateEventBannerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.OrganizerEventById + "/banner", async (
            [FromRoute] Guid eventId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventBannerCommand(eventId, file),
                cancellationToken);

            return result.ToOk("Event banner updated successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UpdateEventBanner")
        .WithSummary("Update event banner")
        .WithDescription("Upload a new banner image for an event.")
        .DisableAntiforgery()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
