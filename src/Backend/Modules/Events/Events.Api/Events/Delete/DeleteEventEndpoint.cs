using Carter;
using Events.Application.Events.Commands.DeleteEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Delete;

public class DeleteEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.EventById, async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new DeleteEventCommand(eventId),
                cancellationToken);

            return result.ToOk("Delete event successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("DeleteEvent")
        .WithSummary("Delete an event")
        .WithDescription("Permanently deletes the event. Only draft events can be deleted.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AllExceptAttendee);
    }
}