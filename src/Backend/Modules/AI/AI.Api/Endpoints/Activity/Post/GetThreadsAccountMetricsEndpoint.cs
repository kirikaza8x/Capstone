using AI.Application.Features.Post.Queries.GetThreadsMetrics;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace AI.Api.Endpoints.Activity.Post;

public sealed class GetThreadsAccountMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/me/threads_insights", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetThreadsAccountMetricsQuery();
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Posts")
        .WithName("GetThreadsAccountMetrics")
        .WithSummary("Fetch Threads aggregate account metrics");
    }
}