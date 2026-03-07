using Carter;
using Events.Application.Events.Commands.CancelEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events;

public class CancelEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/cancel", async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CancelEventCommand(eventId),
                cancellationToken);

            return result.ToOk("Cancel event successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("CancelEvent")
        .WithSummary("Cancel an event")
        .WithDescription("Cancels the event. Only draft or published events can be cancelled.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}