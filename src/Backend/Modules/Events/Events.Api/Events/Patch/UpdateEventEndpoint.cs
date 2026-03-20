using Carter;
using Events.Application.Events.Commands.UpdateEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record UpdateActorImageRequest(
    string Name,
    string? Major,
    string? Image);

public sealed record UpdateEventRequest(
    string? Title,
    List<int>? HashtagIds,
    List<int>? CategoryIds,
    string? Location,
    string? MapUrl,
    string? Description,
    List<UpdateActorImageRequest>? ActorImages);

public class UpdateEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.EventById, async (
            [FromRoute] Guid eventId,
            [FromBody] UpdateEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var actorImages = request.ActorImages?
                .Select(a => new UpdateActorImageItem(a.Name, a.Major, a.Image))
                .ToList();

            var result = await sender.Send(
                new UpdateEventCommand(
                    eventId,
                    request.Title,
                    request.HashtagIds,
                    request.CategoryIds,
                    request.Location,
                    request.MapUrl,
                    request.Description,
                    actorImages),
                cancellationToken);

            return result.ToOk("Event updated successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UpdateEvent")
        .WithSummary("Update event")
        .WithDescription("Partially update basic information of an existing event. Only provided fields will be updated.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
