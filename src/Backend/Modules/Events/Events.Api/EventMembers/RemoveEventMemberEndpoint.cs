using Carter;
using Events.Application.EventMembers.Commands.RemoveEventMember;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Delete;

public class RemoveEventMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.StaffById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid staffId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new RemoveEventMemberCommand(eventId, staffId),
                cancellationToken);

            return result.ToOk("Member removed successfully.");
        })
        .WithTags(Constants.Tags.Member)
        .WithName("RemoveEventMember")
        .WithSummary("Remove event member")
        .WithDescription("Remove a event member from an event.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}