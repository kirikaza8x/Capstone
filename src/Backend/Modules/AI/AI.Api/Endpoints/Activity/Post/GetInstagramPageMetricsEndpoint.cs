using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Marketing.Api.Features.Posts.GetInstagramPageMetrics;

public class GetInstagramPageMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/instagram/metrics", async (
            [FromQuery] InstagramPeriod period,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var query = new GetInstagramPageMetricsQuery(Period: period);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        // .RequireAuthorization()
        .WithTags("Posts")
        .WithName("GetInstagramPageMetrics")
        .WithSummary("Fetch aggregate Instagram account metrics");
    }
}