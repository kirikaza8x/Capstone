using AI.Application.Abstractions;
using AI.Infrastructure.Configs;
using GenerativeAI;
using GenerativeAI.Types;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

            var structuredPrompt = $"""
            IMPORTANT INSTRUCTIONS:
            1. Return ONLY valid JSON matching the expected schema.
            2. Do not include Markdown formatting (no ```json or ``` wrappers).
            3. Do not include explanations, apologies, or extra text before/after the JSON.

            Request:
            {userPrompt}
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
}