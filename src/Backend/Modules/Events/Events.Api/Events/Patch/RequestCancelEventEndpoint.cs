using Carter;
using Events.Application.Events.Commands.RequestCancelEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record RequestCancelEventRequest(string Reason);

public class RequestCancelEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/request-cancellation", async (
            Guid eventId,
            [FromBody] RequestCancelEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new RequestCancelEventCommand(eventId, request.Reason),
                cancellationToken);

            return result.ToOk("Send request successfully");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("RequestCancelEvent")
        .WithSummary("Request event cancellation")
        .WithDescription("Organizer submits a cancellation request with a reason.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}