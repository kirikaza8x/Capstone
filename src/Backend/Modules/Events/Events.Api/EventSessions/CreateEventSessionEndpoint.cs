using Carter;
using Events.Application.EventSessions.Commands.CreateEventSession;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.EventSessions;

public sealed record CreateEventSessionRequestItem(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime);

public sealed record CreateEventSessionRequest(
    List<CreateEventSessionRequestItem> Sessions);

public class CreateEventSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Sessions, async (
            [FromRoute] Guid eventId,
            [FromBody] CreateEventSessionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateEventSessionCommand(
                    eventId,
                    request.Sessions
                        .Select(s => new CreateEventSessionItem(
                            s.Title,
                            s.Description,
                            s.StartTime,
                            s.EndTime))
                        .ToList()),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToCreated(
                $"/api/events/{eventId}/sessions",
                "Event sessions created successfully.");
        })
        .WithTags(Constants.Tags.Sessions)
        .WithName("CreateEventSession")
        .WithSummary("Create event sessions")
        .WithDescription("Creates one or more sessions for an event in a single request.")
        .Produces<ApiResult<List<Guid>>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
