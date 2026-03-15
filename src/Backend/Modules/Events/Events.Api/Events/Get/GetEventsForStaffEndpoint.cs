using Carter;
using Events.Application.Events.Queries.GetEventsForStaff;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Get;

public class GetEventsForStaffEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{Constants.Routes.Events}/pending", async (
            [AsParameters] GetEventsForStaffQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.EventForStaff)
        .WithName("GetEventsForStaff")
        .WithSummary("Get events that need staff attention")
        .WithDescription("Retrieve events with status PendingReview or PendingRequest.")
        .Produces<ApiResult<PagedResult<EventsForStaffResponse>>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Staff);
    }
}