using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reports.Application.Admin.Queries.GetEventRevenueDetails;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Reports.Api.Admin;

public sealed class GetEventRevenueDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Reports.Api.Constants.Routes.AdminEventRevenueDetails, async (
            Guid eventId,
            RevenueTimePeriod? period,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetEventRevenueDetailsQuery(
                EventId: eventId,
                Period: period ?? RevenueTimePeriod.Week);

            var result = await sender.Send(query, cancellationToken);
            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Reports.Api.Constants.Tags.Admin)
        .WithName("GetAdminEventRevenueDetails")
        .WithSummary("Get detailed revenue analytics for an event")
        .Produces<ApiResult<EventRevenueDetailsResponse>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.Admin);
    }
}
