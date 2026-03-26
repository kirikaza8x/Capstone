using Carter;
using Events.Application.EventMembers.Commands.AddEventMember;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Post;

public sealed record AddEventMemberRequest(string Email, List<string> Permissions);

public class AddEventMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.OrganizerEventMember, async (
            [FromRoute] Guid eventId,
            [FromBody] AddEventMemberRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new AddEventMemberCommand(eventId, request.Email, request.Permissions),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToCreated(
                $"/api/events/{eventId}/staff/{result.Value}",
                "Member added successfully.");
        })
        .WithTags(Constants.Tags.Member)
        .WithName("AddEventMember")
        .WithSummary("Add event member")
        .WithDescription("Add a registered Attendee as event member by email.")
        .Produces<ApiResult<Guid>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.Organizer);
    }
}
