using Carter;
using Events.Application.EventMembers.Queries.GetAssignedEvents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.EventMembers.Get;

public sealed class GetAssignedEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.AssignedEvent, async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAssignedEventsQuery();

            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Member)
        .WithName("GetAssignedEvents")
        .WithSummary("Get assigned events or organizer published upcoming/ongoing events")
        .WithDescription("Organizer: published upcoming/ongoing events by start date. Others: assigned active-member events.")
        .Produces<ApiResult<IReadOnlyCollection<AssignedEventResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireRoles(Roles.AttendeeAndOrganizer);
    }
}
