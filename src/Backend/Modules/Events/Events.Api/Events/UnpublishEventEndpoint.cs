using Carter;
using Events.Application.Events.Commands.UnpublishEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events;

public class UnpublishEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/unpublish", async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UnpublishEventCommand(eventId),
                cancellationToken);

            return result.ToNoContent();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UnpublishEvent")
        .WithSummary("Unpublish an event")
        .WithDescription("Changes the event status from Published back to Draft. Only published events can be unpublished.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}