using Carter;
using Events.Application.EventMembers.Commands.UpdateEventMemberPermissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record UpdateEventMemberPermissionsRequest(List<string> Permissions);

public class UpdateEventMemberPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.StaffById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid staffId,
            [FromBody] UpdateEventMemberPermissionsRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventMemberPermissionsCommand(eventId, staffId, request.Permissions),
                cancellationToken);

            return result.ToOk("Permissions updated successfully.");
        })
        .WithTags(Constants.Tags.Member)
        .WithName("UpdateEventMemberPermissions")
        .WithSummary("Update member permissions")
        .WithDescription("Update permissions of an event member.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
