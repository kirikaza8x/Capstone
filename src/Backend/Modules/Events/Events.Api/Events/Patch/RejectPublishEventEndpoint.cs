using Carter;
using Events.Application.Events.Commands.RejectPublishEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record RejectPublishEventRequest(string Reason);

public class RejectPublishEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.StaffEventById}/reject-publish", async (
            Guid eventId,
            [FromBody] RejectPublishEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new RejectPublishEventCommand(eventId, request.Reason),
                cancellationToken);

            return result.ToOk("Publish request rejected successfully.");
        })
        .WithTags(Constants.Tags.EventForStaff)
        .WithName("RejectPublishEvent")
        .WithSummary("Reject event publish request")
        .WithDescription("Admin or Staff rejects an organizer's publish request.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
