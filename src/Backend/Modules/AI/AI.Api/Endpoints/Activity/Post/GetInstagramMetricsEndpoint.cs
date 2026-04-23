using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Queries;
using Shared.Application.Abstractions.Authentication;

namespace Marketing.Api.Features.Posts.GetInstagramMetrics;

public class GetInstagramMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/posts/{postId:guid}/distributions/{distributionId:guid}/instagram-metrics", async (
            Guid postId,
            Guid distributionId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var query = new GetInstagramMetricsQuery(
                PostId: postId,
                DistributionId: distributionId
            );

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        // .RequireAuthorization()
        .WithTags("Posts")
        .WithName("GetInstagramMetrics")
        .WithSummary("Fetch Instagram metrics for a specific distributed post");
    }
}