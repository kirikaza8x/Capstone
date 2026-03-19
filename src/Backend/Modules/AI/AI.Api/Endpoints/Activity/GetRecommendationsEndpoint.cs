using AI.Application.Features.Recommendations.Queries;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace AI.Api.Features.Activity;

public class RecommendationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/activity/recommendations/{userId:guid}", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken,
            int topN = 20,
            bool futureOnly = false) =>
        {
            var query = new GetRecommendationsQuery(userId, topN, futureOnly);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Activity")
        .WithName("GetRecommendations")
        .WithSummary("Get personalised event recommendations for a user")
        .WithDescription("""
            Returns ranked event IDs based on the user's behavior vectors and interest scores.
            
            Source field indicates which path was used:
              - semantic         → personalised via behavior vectors + interest scores
              - category_fallback → personalised via interest scores only
              - popular_fallback  → cold-start, globally popular categories
            """)
        .Produces<List<EventRecommendationResult>>(StatusCodes.Status200OK);
    }
}
