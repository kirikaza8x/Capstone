using Carter;
using Events.Application.Events.Commands.RejectCancelEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record RejectCancelEventRequest(string Reason);

public class RejectCancelEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.StaffEventById}/reject-cancellation", async (
            Guid eventId,
            [FromBody] RejectCancelEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new RejectCancelEventCommand(eventId, request.Reason),
                cancellationToken);

            return result.ToOk("Cancellation request rejected successfully.");
        })
        .WithTags(Constants.Tags.EventForStaff)
        .WithName("RejectCancelEvent")
        .WithSummary("Reject event cancellation request")
        .WithDescription("Admin or Staff rejects an organizer's cancellation request.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
