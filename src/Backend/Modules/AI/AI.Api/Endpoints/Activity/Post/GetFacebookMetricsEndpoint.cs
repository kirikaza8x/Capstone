// Marketing.Api/Features/Posts/GetFacebookMetrics/GetFacebookMetricsEndpoint.cs

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Queries;

namespace Marketing.Api.Features.Posts.GetFacebookMetrics;

public class GetFacebookMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/posts/{postId:guid}/distributions/{distributionId:guid}/metrics/facebook", async (
            Guid postId,
            Guid distributionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFacebookMetricsQuery(
                PostId: postId,
                DistributionId: distributionId);

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        // .RequireAuthorization()
        .WithTags("Posts")
        .WithName("GetFacebookMetrics")
        .WithSummary("Fetch live Facebook metrics for a specific distribution of a post");
    }
}