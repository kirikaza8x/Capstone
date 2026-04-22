using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reports.Application.Admin.Queries.GetFundFlow;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Reports.Api.Admin;

public sealed class GetFundFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.AdminFundFlow, async (
            FundFlowPeriod? period,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFundFlowQuery(period ?? FundFlowPeriod.Month);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.Admin)
        .WithName("GetAdminFundFlow")
        .WithSummary("Get fund flow breakdown by week, month, or quarter with previous-period comparison")
        .Produces<ApiResult<FundFlowResponse>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}
