using Carter;
using Events.Application.Events.Commands.CancelEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record CancelEventRequest(string? Reason);

public class CancelEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/cancel", async (
            Guid eventId,
            [FromBody] CancelEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CancelEventCommand(eventId, request.Reason),
                cancellationToken);

            return result.ToOk("Event cancelled successfully!");
        })
        .WithTags(Constants.Tags.EventForStaff)
        .WithName("CancelEvent")
        .WithSummary("Cancel an event")
        .WithDescription("Admin or Staff cancels an event. Can be used to approve an organizer's cancellation request or force-cancel for policy violations.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
