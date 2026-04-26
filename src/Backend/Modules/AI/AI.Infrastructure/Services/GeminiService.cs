using System.Runtime.CompilerServices;
using System.Text.Json;
using AI.Application.Abstractions;
using AI.Infrastructure.Configs;
using GenerativeAI;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Infrastructure.ExternalServices;

/// <summary>
/// Production implementation of IGeminiService using Google's Generative AI SDK.
/// </summary>
public sealed class GeminiService : IGeminiService
{
    private readonly ILogger<GeminiService> _logger;
    private readonly GeminiConfig _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public GeminiService(IOptions<GeminiConfig> options, ILogger<GeminiService> logger)
    {
        _logger = logger;
        _config = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            throw new InvalidOperationException("Gemini Configuration Error: 'ApiKey' is missing or empty.");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    /// <inheritdoc />
    public async Task<string> GenerateTextAsync(
        string userPrompt,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveSystemInstruction = string.IsNullOrWhiteSpace(systemPromptOverride)
                ? _config.SystemInstruction
                : systemPromptOverride;

            var model = CreateModelWithSystemInstruction(effectiveSystemInstruction);

            var response = await model.GenerateContentAsync(userPrompt, cancellationToken: cancellationToken);

            var result = response.Text ?? string.Empty;

            _logger.LogDebug("GenerateTextAsync completed. Length: {Length}", result.Length);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("GenerateTextAsync cancelled");
            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            var preview = userPrompt.Length > 100 ? userPrompt[..100] + "..." : userPrompt;
            _logger.LogError(ex, "GenerateTextAsync failed. Prompt preview: {Preview}", preview);
            throw new InvalidOperationException("Gemini text generation failed.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResponse> GenerateStructuredAsync<TResponse>(
        string userPrompt,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        try
        {
            var effectiveSystemInstruction = string.IsNullOrWhiteSpace(systemPromptOverride)
                ? _config.SystemInstruction
                : systemPromptOverride;

            var structuredPrompt = $$"""
            Return ONLY a valid JSON object with exactly this shape. No explanation, no markdown, no code fences.

            {
            "title": "Tiêu đề bài viết",
            "summary": "Tóm tắt ngắn 1-2 câu",
            "body": "<JSON array of content blocks as a STRING>"
            }

            The "body" field must be a JSON-serialized STRING containing an array of content blocks:
            - { "type": "heading", "level": 1|2|3, "text": "..." }
            - { "type": "paragraph", "text": "..." }
            - { "type": "image", "src": "<url>", "alt": "..." }   ← only if image provided
            - { "type": "button", label: "...", "href": "..." }
            - { "type": "list", "ordered": false, "items": ["...", "..."] }
            - { "type": "divider" }
            - { "type": "highlight", "content": "..." }

            Rules:
            - All text content must be in Vietnamese.
            - image block src must be exactly the URL provided, never fabricate image URLs.
            - button href must be the real CTA link, never a placeholder.
            - Produce a complete, well-structured marketing post (heading → content → CTA).
            - "body" must be a valid JSON string (escaped), not a nested object.

            Request:
            {{userPrompt}}
            """;

            var model = CreateModelWithSystemInstruction(effectiveSystemInstruction);

            var response = await model.GenerateContentAsync(structuredPrompt, cancellationToken: cancellationToken);
            var rawText = response.Text;

            if (string.IsNullOrWhiteSpace(rawText))
            {
                _logger.LogError("Gemini returned empty response for structured generation.");
                throw new InvalidOperationException("Gemini returned empty response.");
            }

            var cleanJson = CleanJsonString(rawText);

            var result = JsonSerializer.Deserialize<TResponse>(cleanJson, _jsonOptions);

            if (result is null)
            {
                _logger.LogError("Deserialization returned null. JSON preview: {Preview}",
                    cleanJson.Length > 200 ? cleanJson[..200] + "..." : cleanJson);
                throw new InvalidOperationException("Deserialized null result from valid JSON.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed for structured generation");
            throw new InvalidOperationException("Gemini returned invalid or unparseable JSON.", ex);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("GenerateStructuredAsync cancelled");
            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "GenerateStructuredAsync failed unexpectedly");
            throw new InvalidOperationException("Structured generation failed.", ex);
        }
    }


    /// <inheritdoc />
    public async Task<GeminiStructuredResult<TResponse>> GenerateStructuredV2Async<TResponse>(
    string userPrompt,
    string? systemPromptOverride = null,
    CancellationToken cancellationToken = default)
    where TResponse : class
    {
        try
        {
            var effectiveSystemInstruction = string.IsNullOrWhiteSpace(systemPromptOverride)
                ? _config.SystemInstruction
                : systemPromptOverride;

            var structuredPrompt = $$"""
            Return ONLY a valid JSON object with exactly this shape. No explanation, no markdown, no code fences.

            {
            "title": "Tiêu đề bài viết",
            "summary": "Tóm tắt ngắn 1-2 câu",
            "body": "<JSON array of content blocks as a STRING>"
            }

            The "body" field must be a JSON-serialized STRING containing an array of content blocks:
            - { "type": "heading", "level": 1|2|3, "text": "..." }
            - { "type": "paragraph", "text": "..." }
            - { "type": "image", "src": "<url>", "alt": "..." }   ← only if image provided
            - { "type": "button", label: "...", "href": "..." }
            - { "type": "list", "ordered": false, "items": ["...", "..."] }
            - { "type": "divider" }
            - { "type": "highlight", "content": "..." }

            Rules:
            - All text content must be in Vietnamese.
            - image block src must be exactly the URL provided, never fabricate image URLs.
            - button href must be the real CTA link, never a placeholder.
            - Produce a complete, well-structured marketing post (heading → content → CTA).
            - "body" must be a valid JSON string (escaped), not a nested object.

            Request:
            {{userPrompt}}
            """;

            var model = CreateModelWithSystemInstruction(effectiveSystemInstruction);

            // API Call
            var response = await model.GenerateContentAsync(structuredPrompt, cancellationToken: cancellationToken);

            // Extract raw text
            var rawText = response.Text;

            // Capture token usage from SDK 3.6.3 UsageMetadata
            var usage = response.UsageMetadata;
            int promptTokens = usage?.PromptTokenCount ?? 0;
            int candidateTokens = usage?.CandidatesTokenCount ?? 0;
            int totalTokens = usage?.TotalTokenCount ?? 0;

            if (string.IsNullOrWhiteSpace(rawText))
            {
                _logger.LogError("Gemini returned empty response.");
                throw new InvalidOperationException("Gemini returned empty response.");
            }

            var cleanJson = CleanJsonString(rawText);

            // Deserialization
            var result = JsonSerializer.Deserialize<TResponse>(cleanJson, _jsonOptions);

            if (result is null)
            {
                _logger.LogError("Deserialization returned null. JSON: {Preview}",
                    cleanJson.Length > 100 ? cleanJson[..100] : cleanJson);
                throw new InvalidOperationException("Deserialized null result.");
            }

            // Return the full record
            return new GeminiStructuredResult<TResponse>(result, promptTokens, candidateTokens, totalTokens);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed for structured generation.");
            throw new InvalidOperationException("Gemini returned invalid JSON.", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // LOG THE ACTUAL ERROR so you can see it in the console/debug
            _logger.LogError(ex, "GenerateStructuredAsync failed: {Message}", ex.Message);

            // Re-throw so the handler knows something went wrong
            throw;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamChatAsync(
        string userMessage,
        string? systemPromptOverride = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage, nameof(userMessage));

        var effectiveSystemInstruction = string.IsNullOrWhiteSpace(systemPromptOverride)
            ? _config.SystemInstruction
            : systemPromptOverride;

        var model = CreateModelWithSystemInstruction(effectiveSystemInstruction);

        var stream = model.StreamContentAsync(userMessage, cancellationToken: cancellationToken);

        await foreach (var chunk in stream.ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (!string.IsNullOrEmpty(chunk.Text))
            {
                yield return chunk.Text;
            }
        }
    }

    /// <summary>
    /// Creates a GenerativeModel instance with the specified system instruction.
    /// </summary>
    private GenerativeModel CreateModelWithSystemInstruction(string? systemInstruction)
    {
        var config = new GenerationConfig();

        // Only set values if they exist in your GeminiConfig (avoid CS1061)
        // If your config doesn't have these, just use defaults:
        // config.Temperature = 0.1f; // Uncomment if property exists

        return new GenerativeModel(
            apiKey: _config.ApiKey,
            model: _config.Model ?? "gemini-1.5-flash",
            systemInstruction: string.IsNullOrWhiteSpace(systemInstruction) ? null : systemInstruction

        );
    }

    /// <summary>
    /// Cleans LLM response text by removing markdown code block wrappers.
    /// </summary>
    private static string CleanJsonString(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        text = text.Trim();

        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            text = text[7..];
        else if (text.StartsWith("```"))
            text = text[3..];

        if (text.EndsWith("```"))
            text = text[..^3];

        return text.Trim();
    }

    public string GetModelInfo()
    {
        return $"{_config.Model}";
    }
}
