using Shared.Domain.Abstractions;
using Shared.Application.Abstractions.Messaging;
using AI.Application.Features.Recommendations.Queries;
using AI.Application.Features.Recommendations.DTOs;
using AI.Application.Abstractions;
using AI.Domain.Repositories;
using Events.PublicApi.PublicApi;
using Microsoft.Extensions.Logging;

namespace AI.Application.Features.Recommendations.Handlers;

/// <summary>
/// Handles recommendation queries by fetching user interests, candidate events,
/// and ranking them via AI with graceful fallbacks.
/// </summary>
public sealed class GetRecommendationsQueryHandler
    : IQueryHandler<GetRecommendationsProtoQuery, List<RecommendationResultLiteDto>>
{
    private const int MaxCategoriesToFetch = 5;
    private const int MaxCandidatesToRank = 30;
    private const int DefaultTopN = 10;

    private readonly IUserInterestScoreRepository _interestRepository;
    private readonly IEventMemberPublicApi _eventApi;
    private readonly IRecommendationAiService _aiService;
    private readonly ILogger<GetRecommendationsQueryHandler> _logger;

    public GetRecommendationsQueryHandler(
        IUserInterestScoreRepository interestRepository,
        IEventMemberPublicApi eventApi,
        IRecommendationAiService aiService,
        ILogger<GetRecommendationsQueryHandler> logger)
    {
        _interestRepository = interestRepository;
        _eventApi = eventApi;
        _aiService = aiService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<RecommendationResultLiteDto>>> Handle(
        GetRecommendationsProtoQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var interests = await _interestRepository
                .GetTopCategoriesAsync(request.UserId, MaxCategoriesToFetch, 0, cancellationToken);

            var categories = interests
                .Select(x => x.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            var candidates = await _eventApi
                .GetEventsByCategoriesOrHashtagsAsync(
                    categories,
                    [],  
                    cancellationToken);

            if (candidates.Count == 0)
            {
                _logger.LogDebug("No candidates found for user {UserId}", request.UserId);
                return Result.Success(new List<RecommendationResultLiteDto>());
            }

            candidates = candidates.Take(MaxCandidatesToRank).ToList();

            IReadOnlyList<int> rankedIndexes;
            try
            {
                rankedIndexes = await _aiService
                    .RankEventsAsync(candidates, cancellationToken);
                
                _logger.LogDebug("AI returned {Count} ranked indexes", rankedIndexes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI ranking failed for user {UserId}; falling back to unranked", request.UserId);
                rankedIndexes = Enumerable.Range(0, candidates.Count).ToList();
            }

            var results = rankedIndexes
                .Where(i => i >= 0 && i < candidates.Count)  
                .Distinct()                                   
                .Select(i => candidates[i])
                .Select(e => new RecommendationResultLiteDto
                {
                    EventId = e.Id,
                    Title = e.Title ?? string.Empty,
                    BannerUrl = e.BannerUrl,
                    EventStartAt = e.EventStartAt,
                    MinPrice = e.MinPrice
                })
                .Take(request.TopN > 0 ? request.TopN : DefaultTopN)  
                .ToList();

            _logger.LogInformation("Returned {Count} recommendations for user {UserId}", 
                results.Count, request.UserId);

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle recommendation query for user {UserId}", request.UserId);
            
            return Result.Failure<List<RecommendationResultLiteDto>>(
                 Error.Failure("Recommendations.FetchFailed", "Unable to fetch recommendations at this time"));
        }
    }
}