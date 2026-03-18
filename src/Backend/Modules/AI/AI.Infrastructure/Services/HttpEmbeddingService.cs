using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Embbeding;
using System.Net.Http.Json;
using System.Text.Json;

namespace AI.Infrastructure.Embedding;

/// <summary>
/// HTTP implementation of IEmbeddingService — calls Python FastAPI directly.
///
/// USE FOR: local development and debugging before switching to RabbitMQ.
/// ENDPOINT: POST /embeddings/generate
///
/// To swap back to RabbitMQ: change DI registration only, no handler changes needed.
/// </summary>
public sealed class HttpEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpEmbeddingService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HttpEmbeddingService(
        HttpClient http,
        ILogger<HttpEmbeddingService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));

        var trimmed = text.Length > 512 ? text[..512] : text;

        var request = new { text = trimmed, normalize = true };

        _logger.LogDebug("Sending embed request for text: {Preview}...", trimmed[..Math.Min(50, trimmed.Length)]);

        var response = await _http.PostAsJsonAsync("/embeddings/generate", request, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Embedding service returned {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GenerateEmbeddingResponse>(JsonOptions, ct);

        if (result is null || !result.Success || result.Embedding is null)
            throw new InvalidOperationException("Embedding service returned empty or failed response.");

        _logger.LogDebug("Received {Dim}-dim embedding", result.Dimension);

        return result.Embedding.ToArray();
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken ct = default)
    {
        if (texts.Count == 0) return Array.Empty<float[]>();

        var trimmed = texts.Select(t => t.Length > 512 ? t[..512] : t).ToList();
        var request = new { texts = trimmed, normalize = true };

        var response = await _http.PostAsJsonAsync("/embeddings/batch", request, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Batch embedding service returned {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<BatchEmbeddingResponse>(JsonOptions, ct);

        if (result is null || !result.Success || result.Embeddings is null)
            throw new InvalidOperationException("Batch embedding service returned empty or failed response.");

        _logger.LogDebug("Received batch of {Count} embeddings", result.Count);

        return result.Embeddings.Select(e => e.ToArray()).ToList();
    }

    // ── Response DTOs — mirrors Python schemas exactly ────────────

    private sealed record GenerateEmbeddingResponse(
        bool          Success,
        string        Text,
        List<float>   Embedding,
        int           Dimension,
        bool          Normalized,
        string        Model
    );

    private sealed record BatchEmbeddingResponse(
        bool              Success,
        int               Count,
        List<List<float>> Embeddings,
        int               Dimension,
        bool              Normalized,
        string            Model
    );
}

/// <summary>
/// Config for the HTTP embedding service.
/// </summary>
public sealed class HttpEmbeddingOptions
{
    public const string Section = "Embedding:Http";

    /// <summary>Base URL of the Python FastAPI server. e.g. http://localhost:8000</summary>
    public string BaseUrl { get; set; } = "http://localhost:8000";

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}