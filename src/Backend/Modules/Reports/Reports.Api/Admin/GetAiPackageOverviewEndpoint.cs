using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reports.Application.Admin.Queries.GetAiPackageOverview;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Reports.Api.Endpoints.AdminDashboards;

public sealed class GetAiPackageOverviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.AdminAiPackageOverview, async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAiPackageOverviewQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Admin)
        .WithName("GetAdminAiPackageOverview")
        .WithSummary("Get AI package revenue and most active package for admin dashboard")
        .Produces<ApiResult<AiPackageOverviewResponse>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}
