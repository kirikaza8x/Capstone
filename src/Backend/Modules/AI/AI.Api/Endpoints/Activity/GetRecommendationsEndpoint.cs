using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using AI.Application.Features.Recommendations.Queries;
using AI.Application.Features.Recommendations.DTOs;

namespace AI.Api.Features.Activity
{
    public class RecommendationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/activity/recommendations/{userId:guid}", async (
                Guid userId,
                [AsParameters] RecommendationRequestParams parameters,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetRecommendationsProtoQuery(userId, parameters.Count ?? 10);
                var result = await sender.Send(query, cancellationToken);
                return result.ToOk();
            })
            .WithTags("Activity")
            .WithName("GetRecommendations")
            .WithSummary("Get personalized recommendations")
            .Produces<List<RecommendationResultDto>>(StatusCodes.Status200OK);

        }
    }

    public record RecommendationRequestParams(int? Count);
}
