using AI.Application.Features.Post.Queries.GetThreadsMetrics;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace AI.Api.Endpoints.Activity.Post;

public sealed class GetThreadsMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/threads/{mediaId}/insights", async (
            string mediaId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetThreadsMetricsQuery(mediaId);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Posts")
        .WithName("GetThreadsMetrics")
        .WithSummary("Fetch Threads metrics for a specific media post");
    }
}