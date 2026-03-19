using Carter;
using Events.Application.Events.Commands.UpdateEventPolicy;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record UpdateEventPolicyRequest(string Policy);

public class UpdateEventPolicyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/policy", async (
            [FromRoute] Guid eventId,
            [FromBody] UpdateEventPolicyRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventPolicyCommand(eventId, request.Policy),
                cancellationToken);

            return result.ToOk("Event policy updated successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UpdateEventPolicy")
        .WithSummary("Update event policy")
        .WithDescription("Organizer updates the event policy text (supports multiline content).")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
