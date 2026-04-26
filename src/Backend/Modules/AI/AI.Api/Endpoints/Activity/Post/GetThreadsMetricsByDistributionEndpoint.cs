using AI.Application.Features.Post.Queries.GetThreadsMetrics;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace AI.Api.Endpoints.Activity.Post;

public sealed class GetThreadsMetricsByDistributionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/posts/{postId:guid}/distributions/{distributionId:guid}/metrics/threads", async (
            Guid postId,
            Guid distributionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetThreadsMetricsByDistributionQuery(
                PostId: postId,
                DistributionId: distributionId);

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Posts")
        .WithName("GetThreadsMetricsByDistribution")
        .WithSummary("Fetch Threads metrics for a specific distribution of a post");
    }
}