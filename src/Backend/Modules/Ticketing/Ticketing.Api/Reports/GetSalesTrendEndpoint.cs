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
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSalesTrendQuery(
                eventId,
                startDate,
                endDate);

            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetSalesTrend")
        .WithSummary("Get sales trend chart data by date range")
        .WithDescription("Returns daily sales trend from startDate to endDate.")
        .Produces<ApiResult<SalesTrendResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AttendeeAndOrganizer)
        .RequireEventPermission(EventPermissions.ViewReports);
    }
}
