using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Queries;
using Microsoft.AspNetCore.Mvc;
using Marketing.Domain.Enums; 

namespace Marketing.Api.Features.Posts.GetFacebookPageMetrics;

public class GetFacebookPageMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/facebook/page/metrics", async (
            ISender sender,
            [FromQuery] FacebookPeriod period = FacebookPeriod.Day, 
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetFacebookPageMetricsQuery(period);

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        // .RequireAuthorization()
        .WithTags("Facebook")
        .WithName("GetFacebookPageMetrics")
        .WithSummary("Fetch live Facebook metrics for a page");
    }
}