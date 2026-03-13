using Carter;
using Events.Application.EventMembers.Queries.GetEventMembers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Get;

public class GetEventMembersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Staff, async (
            [FromRoute] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetEventMembersQuery(eventId), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Member)
        .WithName("GetEventMembers")
        .WithSummary("Get event members")
        .WithDescription("Retrieve all event members of an event.")
        .Produces<IReadOnlyList<EventMemberResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}