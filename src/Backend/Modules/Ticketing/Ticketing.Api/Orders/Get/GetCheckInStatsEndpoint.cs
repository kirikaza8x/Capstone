using Carter;
using Events.PublicApi.Constants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Api.Extensions;
using Ticketing.Application.Orders.Queries.GetCheckInStats;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Get;

public class GetCheckInStatsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.CheckInStats, async (
            Guid eventId,
            [FromQuery] Guid sessionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCheckInStatsQuery(eventId, sessionId);

            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetCheckInStats")
        .WithSummary("Get check-in statistics")
        .WithDescription("Retrieves real-time check-in statistics for a specific event session, including total checked-in and breakdown by ticket types.")
        .Produces<ApiResult<CheckInStatsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.CheckIn);
    }
}
