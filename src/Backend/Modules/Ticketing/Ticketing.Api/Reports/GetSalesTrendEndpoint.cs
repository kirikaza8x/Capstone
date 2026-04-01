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
using Ticketing.Application.Reports.GetSalesTrend;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Reports.Get;

public class GetSalesTrendEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.SalesTrend, async (
            Guid eventId,
            [FromQuery] SalesTrendPeriod? period,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var safePeriod = period ?? SalesTrendPeriod.Day;

            var query = new GetSalesTrendQuery(eventId, safePeriod);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetSalesTrend")
        .WithSummary("Get sales trend chart data")
        .Produces<ApiResult<SalesTrendResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.ViewReports);
    }
}
