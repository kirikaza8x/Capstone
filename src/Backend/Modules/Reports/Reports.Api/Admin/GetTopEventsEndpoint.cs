using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Reports.Application.Admin.Queries.GetTopEvents;
using Reports.Application.AdminDashboards.Queries.GetTopEvents;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Reports.Api.Endpoints.AdminDashboards;

public class GetTopEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.AdminTopEvents, async (
            [FromQuery] int? top,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTopEventsQuery(top ?? 5);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Admin)
        .WithName("GetAdminTopEvents")
        .WithSummary("Get top grossing events for the admin dashboard")
        .Produces<ApiResult<TopEventsResponse>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}
