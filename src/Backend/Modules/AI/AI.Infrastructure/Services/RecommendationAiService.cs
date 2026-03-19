using System.Text.Json;
using AI.Application.Abstractions;
using AI.Application.Features.Recommendations.DTOs;
using Events.PublicApi.Records;
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.ExternalServices;

/// <summary>
/// AI-powered event ranking service using Gemini for structured JSON responses.
/// </summary>
public sealed class RecommendationAiService : IRecommendationAiService
{
    private readonly IGeminiService _gemini;
    private readonly ILogger<RecommendationAiService>? _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private const string ResponseFormat = """
    {
      "rankedIndexes": [0, 1, 2]
    }
    """;

    public RecommendationAiService(
        IGeminiService gemini,
        ILogger<RecommendationAiService>? logger = null)
    {
        _gemini = gemini;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> RankEventsAsync(
        IReadOnlyList<EventRecommendationFeature> candidates,
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            _logger?.LogDebug("RankEventsAsync called with empty candidates list");
            return [];
        }

        var json = JsonSerializer.Serialize(candidates, _jsonOptions);

        // (Note: This is passed as systemPromptOverride, but prefer setting default in GeminiConfig)
        var systemPrompt = """
        You are an event recommendation AI.

        Rank the events from most relevant to least relevant
        based on category, hashtags, event date, and price.

        Return ONLY JSON matching the requested format.
        """;

        var userPrompt = $"""
        Candidate events:

        {json}

        Return format:

        {ResponseFormat}
        """;

        _logger?.LogDebug("Sending {Count} candidates to Gemini for ranking", candidates.Count);

        // GenerateStructuredAsync<T>(string userPrompt, string? systemPromptOverride, CancellationToken)
        var response = await _gemini.GenerateStructuredAsync<GeminiRecommendationResponse>(
            userPrompt,
            systemPrompt,
            cancellationToken);

        if (response?.RankedIndexes == null)
        {
            _logger?.LogWarning("Gemini returned null or empty RankedIndexes");
            return [];
        }

        _logger?.LogDebug("Gemini returned {Count} ranked indexes", response.RankedIndexes.Count);

        return response.RankedIndexes;
    }
}
