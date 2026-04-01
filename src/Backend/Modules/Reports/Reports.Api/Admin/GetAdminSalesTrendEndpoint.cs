using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reports.Application.Admin.Queries.GetSalesTrend;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Reports.Api.Endpoints.AdminDashboards;

public class GetAdminSalesTrendEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.AdminSalesTrend, async (
            int? days,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAdminSalesTrendQuery(days ?? 30);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Admin)
        .WithName("GetAdminSalesTrend")
        .WithSummary("Get daily sales and transaction trend for charting")
        .Produces<ApiResult<AdminSalesTrendResponse>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}
