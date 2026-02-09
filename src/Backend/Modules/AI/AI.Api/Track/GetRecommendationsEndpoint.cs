using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AI.Application.Features.Recommendations.Queries;
using AI.Application.Features.Recommendations.DTOs;

namespace AI.Api.Features.Recommendations
{
    public class GetRecommendationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/ai/recommendations/{userId:guid}", async (
                Guid userId,
                [AsParameters] RecommendationRequestParams parameters, // For query strings
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetRecommendationsQuery(userId, parameters.Count ?? 10);

                var result = await sender.Send(query, cancellationToken);

                // Assuming you have a ToOk() extension method like in your User module
                // If not: return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithTags("AI Recommendations")
            .WithName("GetRecommendations")
            .WithSummary("Get personalized recommendations for a user")
            .Produces<List<RecommendationResultDto>>(StatusCodes.Status200OK);
        }
    }

    public record RecommendationRequestParams(int? Count);
}