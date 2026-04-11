using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Application.Abstractions;
using AI.Application.Features.ImageGeneration;
using AI.Infrastructure.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Storage;

namespace AI.Infrastructure.ExternalServices;

public sealed class OpenRouterImageService : IImageGenerationService
{
    private const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly OpenRouterConfig _config;
    private readonly IStorageService _storage;
    private readonly ILogger<OpenRouterImageService> _logger;

    public OpenRouterImageService(
        HttpClient http,
        IOptions<OpenRouterConfig> options,
        IStorageService storage,
        ILogger<OpenRouterImageService> logger)
    {
        _http    = http;
        _config  = options.Value;
        _storage = storage;
        _logger  = logger;
    }

    public async Task<ImageGenerationResult> GenerateImagesAsync(
        ImageGenerationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var payload = new OpenRouterRequest
        {
            Model      = _config.Model,
            Messages   = [new() { Role = "user", Content = request.Prompt }],
            Modalities = ["image"],
            ImageConfig = new()
            {
                AspectRatio = request.AspectRatio,
                ImageSize   = request.ImageSize
            }
        };

        var json    = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = content
        };

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _config.ApiKey);

        _logger.LogInformation(
            "OpenRouter image generation started. Model={Model}, Prompt={Prompt}, AspectRatio={AspectRatio}, ImageSize={ImageSize}",
            _config.Model, request.Prompt, request.AspectRatio, request.ImageSize);

        var response = await _http.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenRouter {StatusCode}: {Body}",
                (int)response.StatusCode, errorBody);
            response.EnsureSuccessStatusCode();
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("OpenRouter raw response length: {Length} chars", raw.Length);

        return await ParseAndUploadAsync(raw, cancellationToken);
    }

    private async Task<ImageGenerationResult> ParseAndUploadAsync(
        string raw,
        CancellationToken cancellationToken)
    {
        var root = JsonSerializer.Deserialize<OpenRouterResponse>(raw, JsonOptions)
            ?? throw new InvalidOperationException("Empty response from OpenRouter.");

        foreach (var choice in root.Choices ?? [])
        {
            foreach (var image in choice.Message?.Images ?? [])
            {
                var base64DataUrl = image.ImageUrl?.Url;
                if (string.IsNullOrWhiteSpace(base64DataUrl))
                    continue;

                // data:image/png;base64,<payload>
                // Strip the prefix to get the raw base64 string
                var comma      = base64DataUrl.IndexOf(',');
                var base64Data = comma >= 0
                    ? base64DataUrl[(comma + 1)..]
                    : base64DataUrl;

                // Detect content type from the prefix e.g. "data:image/png;base64"
                var contentType = "image/png";
                if (comma > 0)
                {
                    var prefix = base64DataUrl[..comma]; // "data:image/png;base64"
                    var start  = prefix.IndexOf(':') + 1;
                    var end    = prefix.IndexOf(';');
                    if (start > 0 && end > start)
                        contentType = prefix[start..end];
                }

                var extension = contentType.Split('/').LastOrDefault() ?? "png";
                var fileName  = $"ai-generated-{Guid.NewGuid():N}.{extension}";
                var folder    = "ai-generated";

                var bytes = Convert.FromBase64String(base64Data);
                await using var stream = new MemoryStream(bytes);

                var publicUrl = await _storage.UploadAsync(
                    stream,
                    fileName,
                    contentType,
                    folder,
                    cancellationToken);

                _logger.LogInformation("Image uploaded. PublicUrl={Url}", publicUrl);

                return new ImageGenerationResult { ImageUrl = publicUrl };
            }
        }

        throw new InvalidOperationException("No image was returned from OpenRouter.");
    }

    // ── Request models ────────────────────────────────────────────────────────

    private sealed class OpenRouterRequest
    {
        public string Model { get; set; } = default!;
        public List<Message> Messages { get; set; } = [];
        public List<string> Modalities { get; set; } = [];
        public ImageConfig? ImageConfig { get; set; }
    }

    private sealed class Message
    {
        public string Role { get; set; } = default!;
        public string Content { get; set; } = default!;
    }

    private sealed class ImageConfig
    {
        public string? AspectRatio { get; set; }
        public string? ImageSize { get; set; }
    }

    // ── Response models ───────────────────────────────────────────────────────

    private sealed class OpenRouterResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        public AssistantMessage? Message { get; set; }
    }

    private sealed class AssistantMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public List<Image>? Images { get; set; }
    }

    private sealed class Image
    {
        public ImageUrl? ImageUrl { get; set; }
    }

    private sealed class ImageUrl
    {
        public string? Url { get; set; }
    }
}