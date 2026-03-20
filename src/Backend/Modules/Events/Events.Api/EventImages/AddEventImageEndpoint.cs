using Carter;
using Events.Application.EventImages.AddEventImage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.EventImages;

public class AddEventImageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.EventImages, async (
            [FromRoute] Guid eventId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new AddEventImageCommand(eventId, file),
                cancellationToken);

            return result.ToCreated(
                $"/api/events/{eventId}/images/{result.Value}",
                "Event image added successfully.");
        })
        .WithTags(Constants.Tags.EventImages)
        .WithName("AddEventImage")
        .WithSummary("Add image to event")
        .WithDescription("Upload and add a new image to the specified event. Allowed types: JPEG, PNG, GIF, WebP. Max size: 10MB.")
        .DisableAntiforgery()
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
