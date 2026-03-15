using Carter;
using Events.Application.Events.Queries.GetEventsForAdmin;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Get;

public class GetEventsForAdminEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{Constants.Routes.Events}/admin", async (
            [AsParameters] GetEventsForAdminQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.EventForAdmin)
        .WithName("GetEventsForAdmin")
        .WithSummary("Get all events for administration")
        .WithDescription("Retrieve all events except Drafts. Can filter by organizerId, statuses, and title.")
        .Produces<ApiResult<PagedResult<EventsForAdminResponse>>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}