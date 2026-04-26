using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Reports.GetOrganizerRevenuePerEvent;
using Ticketing.Application.Reports.GetOrganizerRevenueSummary;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Reports;

public class GetOrganizerRevenueSummaryEndpoint : ICarterModule
{
    private const string Route = "api/ticketing/report/organizers/revenue/summary";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Route, async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerRevenueSummaryQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetOrganizerRevenueSummaryInTicketing")
        .WithSummary("Get organizer revenue summary from ticketing")
        .WithDescription("Returns organizer revenue summary with separated gross revenue, discount amount, and net revenue.")
        .Produces<ApiResult<OrganizerRevenueSummaryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.Organizer, Roles.Admin);
    }
}

public class GetOrganizerRevenuePerEventEndpoint : ICarterModule
{
    private const string Route = "api/ticketing/report/organizers/revenue/events";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Route, async (
            bool byNet,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerRevenuePerEventQuery(byNet);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetOrganizerRevenuePerEventInTicketing")
        .WithSummary("Get organizer revenue per event from ticketing")
        .WithDescription("Returns organizer revenue per event with separated gross revenue, discount amount, and net revenue.")
        .Produces<ApiResult<OrganizerRevenuePerEventResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.Organizer, Roles.Admin);
    }
}
