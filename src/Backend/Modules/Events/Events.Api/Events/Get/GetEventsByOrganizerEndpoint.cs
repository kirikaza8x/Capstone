using Carter;
using Events.Application.Events.Queries.GetEvents;
using Events.Application.Events.Queries.GetEventsByOrganizer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Get;

public class GetEventsByOrganizerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{Constants.Routes.Events}/me", async (
            [AsParameters] GetEventsByOrganizerQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("GetMyEvents")
        .WithSummary("Get my events")
        .WithDescription("Retrieve all events of the current organizer with pagination. Filter by `title` and `statuses` (e.g. `?statuses=draft,published`).")
        .Produces<ApiResult<PagedResult<EventResponse>>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Organizer);
    }
}
