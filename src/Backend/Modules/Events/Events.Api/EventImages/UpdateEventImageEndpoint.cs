using Carter;
using Events.Application.Events.Commands.UpdateEventImage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.EventImages;

public class UpdateEventImageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.EventImageById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid imageId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventImageCommand(eventId, imageId, file),
                cancellationToken);

            return result.ToOk("Event image updated successfully.");
        })
        .WithTags(Constants.Events)
        .WithName("UpdateEventImage")
        .WithSummary("Update event image")
        .WithDescription("Replace an existing event image with a new one. Allowed types: JPEG, PNG, GIF, WebP. Max size: 10MB.")
        .DisableAntiforgery()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}