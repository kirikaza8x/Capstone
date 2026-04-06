using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Reports.GetSalesTrendForAllEvent;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Reports;

public class GetSalesTrendForAllEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.SalesTrendForAllEvents, async (
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSalesTrendForAllEventQuery(startDate, endDate);

            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Reports)
        .WithName("GetSalesTrendForAllEvents")
        .WithSummary("Get sales trend for all events of current organizer")
        .WithDescription("Returns per-event daily sales trend from startDate to endDate.")
        .Produces<ApiResult<SalesTrendForAllEventResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.Organizer, Roles.Admin);
    }
}
