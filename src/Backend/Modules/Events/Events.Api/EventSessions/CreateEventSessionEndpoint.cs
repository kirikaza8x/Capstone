using Carter;
using Events.Application.EventSessions.Commands.CreateEventSession;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.EventSessions;

public sealed record CreateEventSessionRequest(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime);

public class CreateEventSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.EventSessions, async (
            [FromRoute] Guid eventId,
            [FromBody] CreateEventSessionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateEventSessionCommand(
                    eventId,
                    request.Title,
                    request.Description,
                    request.StartTime,
                    request.EndTime),
                cancellationToken);

            return result.ToCreated(
                $"/api/events/{eventId}/sessions/{result.Value}",
                "Event session created successfully.");
        })
        .WithTags(Constants.Events)
        .WithName("CreateEventSession")
        .WithSummary("Create a new event session")
        .WithDescription("Creates a new session for an event with specified time range.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
