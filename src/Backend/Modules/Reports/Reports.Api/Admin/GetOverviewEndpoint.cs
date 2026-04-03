using System.Reflection.Metadata;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reports.Application.Admin.Queries.GetOverview;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Reports.Api.Admin;

public class GetOverviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.AdminOverview, async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOverviewQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Admin)
        .WithName("GetAdminOverview")
        .WithSummary("Get aggregated KPIs for the Admin overview dashboard")
        .Produces<ApiResult<OverviewResponse>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}
