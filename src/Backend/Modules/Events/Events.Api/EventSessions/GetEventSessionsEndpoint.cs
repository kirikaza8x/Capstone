using Carter;
using Events.Application.Events.DTOs;
using Events.Application.EventSessions.Queries.GetEventSessions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.EventSessions;

public class GetEventSessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Sessions, async (
            [FromRoute] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetEventSessionsQuery(eventId), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Sessions)
        .WithName("GetEventSessions")
        .WithSummary("Get event sessions")
        .WithDescription("Retrieve all sessions with ticket types for an event.")
        .Produces<IReadOnlyList<EventSessionDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}