using Carter;
using Events.Application.EventImages.DeleteEventImage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.EventImages;

public class DeleteEventImageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.OrganizerEventImageById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid imageId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new DeleteEventImageCommand(eventId, imageId),
                cancellationToken);

            return result.ToNoContent();
        })
        .WithTags(Constants.Tags.EventImages)
        .WithName("DeleteEventImage")
        .WithSummary("Delete event image")
        .WithDescription("Delete an image from the specified event. Also removes the file from storage.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
