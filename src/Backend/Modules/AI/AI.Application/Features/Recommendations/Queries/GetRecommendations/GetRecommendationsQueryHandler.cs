using Shared.Domain.Abstractions;
using AI.Application.Features.Recommendations.Queries;
using AI.Application.Features.Recommendations.DTOs;
using AI.Application.Abstractions;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Recommendations.Handlers
{
    public class GetRecommendationsQueryHandler : IQueryHandler<GetRecommendationsQuery, List<RecommendationResultDto>>
    {
        private readonly IRecommendationService _recommendationService;

        public GetRecommendationsQueryHandler(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        public async Task<Result<List<RecommendationResultDto>>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
        {
            // 1. Get data from service
            var recommendations = await _recommendationService.GetRecommendationsAsync(request.UserId, request.Count);

            // 2. Return Success
            return Result.Success(recommendations);
        }
    }
}