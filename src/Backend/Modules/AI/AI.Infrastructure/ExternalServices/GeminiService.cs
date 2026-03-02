using AI.Application.Abstractions;
using AI.Infrastructure.Configs;
using GenerativeAI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AI.Infrastructure.ExternalServices;

public sealed class GeminiService : IGeminiService
{
    private readonly GenerativeModel _model;
    private readonly ILogger<GeminiService> _logger;
    private readonly GeminiConfig _config;

    public GeminiService(IOptions<GeminiConfig> options, ILogger<GeminiService> logger)
    {
        _logger = logger;
        _config = options.Value;

        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            throw new InvalidOperationException("Gemini Configuration Error: 'ApiKey' is missing.");
        }

        _model = new GenerativeModel(
            apiKey: _config.ApiKey,
            model: _config.Model ?? "gemini-1.5-flash",
            systemInstruction: _config.SystemInstruction
        );
    }

    public async Task<string> GenerateTextAsync(string? systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var fullPrompt = string.IsNullOrEmpty(systemPrompt)
            ? userPrompt
            : $"{systemPrompt}\n\nUser: {userPrompt}";

        try
        {
            var response = await _model.GenerateContentAsync(fullPrompt, cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed.");
            throw; // Let the global handler catch it, or return fallback
        }
    }

    public async Task<TResponse> GenerateStructuredAsync<TResponse>(
        string? systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        var structuredPrompt = $"""
        {systemPrompt}

        IMPORTANT INSTRUCTIONS:
        1. Return ONLY valid JSON.
        2. Do not include Markdown formatting (no ```json or ```).
        3. Do not include explanations.

        Request:
        {userPrompt}
        """;

        var response = await _model.GenerateContentAsync(structuredPrompt, cancellationToken: cancellationToken);
        var rawText = response.Text;

        if (string.IsNullOrWhiteSpace(rawText))
            throw new InvalidOperationException("Gemini returned empty response.");

        var cleanJson = CleanJsonString(rawText);
        try
        {
            return JsonSerializer.Deserialize<TResponse>(cleanJson, JsonOptions)
                   ?? throw new InvalidOperationException("Deserialized null.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from Gemini. Raw: {RawText}", rawText);
            throw new InvalidOperationException("Gemini returned invalid JSON.", ex);
        }
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {

        if (string.IsNullOrWhiteSpace(userMessage))

        {
            yield break;
        }

        var stream = _model.StreamContentAsync(userMessage, cancellationToken: cancellationToken);

        await foreach (var chunk in stream.ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            if (!string.IsNullOrEmpty(chunk.Text))
                yield return chunk.Text;

        }

    }

    private static string CleanJsonString(string text)
    {
        text = text.Trim();

        // Remove markdown code blocks if present
        if (text.StartsWith("```json"))
            text = text.Substring(7);
        if (text.StartsWith("```")) // Sometimes just ```
            text = text.Substring(3);
        if (text.EndsWith("```"))
            text = text.Substring(0, text.Length - 3);

        return text.Trim();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true // Helps with LLM quirks
    };
}