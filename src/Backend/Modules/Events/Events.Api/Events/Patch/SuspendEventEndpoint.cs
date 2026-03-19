using Carter;
using Events.Application.Events.Commands.SuspendEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record SuspendEventRequest(string Reason, int FixWindowHours);

public class SuspendEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/suspend", async (
            Guid eventId,
            [FromBody] SuspendEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new SuspendEventCommand(eventId, request.Reason, request.FixWindowHours),
                cancellationToken);

            return result.ToOk("Event suspended successfully.");
        })
        .WithTags(Constants.Tags.EventForStaff)
        .WithName("SuspendEvent")
        .WithSummary("Suspend an event")
        .WithDescription("Admin or Staff suspends a published event and sets fix window in hours for organizer resubmission.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
