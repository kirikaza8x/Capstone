using Carter;
using Events.Application.EventSessions.Commands.DeleteEventSession;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.EventSessions;

public class DeleteEventSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.SessionById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new DeleteEventSessionCommand(eventId, sessionId),
                cancellationToken);

            return result.ToOk("Session deleted successfully.");
        })
        .WithTags(Constants.Tags.Sessions)
        .WithName("DeleteEventSession")
        .WithSummary("Delete event session")
        .WithDescription("Delete a session from an event.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
