using AI.Application.Abstractions;
using GenerativeAI;
using GenerativeAI.Types;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace Infrastructure.Services.AI;

public sealed class GeminiService : IGeminiService
{
    private readonly GenerativeModel _model;

    public GeminiService(IConfiguration configuration)
    {
        var geminiSection = configuration.GetSection("Gemini");

        var apiKey = geminiSection["ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini ApiKey is missing.");

        var modelName = geminiSection["Model"] ?? "gemini-2.5-flash";
        var systemInstruction = geminiSection["SystemInstruction"];

        _model = new GenerativeModel(
            apiKey: apiKey,
            model: modelName,
            systemInstruction: systemInstruction
        );
    }

    public async Task<string> GenerateTextAsync(
        string? systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(systemPrompt, userPrompt);

        var response = await _model.GenerateContentAsync(
            prompt,
            cancellationToken: cancellationToken
        );

        return response.Text ?? string.Empty;
    }

    public async Task<TResponse> GenerateStructuredAsync<TResponse>(
        string? systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        var structuredPrompt = $"""
        {systemPrompt}

        IMPORTANT:
        - Output MUST be valid JSON
        - No markdown
        - No comments
        - No extra text

        User request:
        {userPrompt}
        """;

        var response = await _model.GenerateContentAsync(
            structuredPrompt,
            cancellationToken: cancellationToken
        );

        if (string.IsNullOrWhiteSpace(response.Text))
            throw new InvalidOperationException("Gemini returned empty response.");

        return JsonSerializer.Deserialize<TResponse>(
            response.Text,
            JsonOptions
        ) ?? throw new InvalidOperationException("Failed to deserialize Gemini response.");
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

    private static string BuildPrompt(string? systemPrompt, string userPrompt)
    {
        return $"""
        SYSTEM:
        {systemPrompt}

        USER:
        {userPrompt}
        """;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
