using Carter;
using Events.Application.EventSessions.Commands.UpdateEventSession;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record UpdateEventSessionRequest(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime);

public class UpdateEventSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.OrganizerSessionById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            [FromBody] UpdateEventSessionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventSessionCommand(
                    eventId,
                    sessionId,
                    request.Title,
                    request.Description,
                    request.StartTime,
                    request.EndTime),
                cancellationToken);

            return result.ToOk("Session updated successfully.");
        })
        .WithTags(Constants.Tags.Sessions)
        .WithName("UpdateEventSession")
        .WithSummary("Update event session")
        .WithDescription("Update an existing session of an event.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
