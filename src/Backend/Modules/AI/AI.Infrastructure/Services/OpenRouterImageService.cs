using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Application.Abstractions;
using AI.Application.Features.ImageGeneration;
using AI.Infrastructure.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    private readonly ILogger<OpenRouterImageService> _logger;

    public OpenRouterImageService(
        HttpClient http,
        IOptions<OpenRouterConfig> options,
        ILogger<OpenRouterImageService> logger)
    {
        _http = http;
        _config = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ImageGenerationResult>> GenerateImagesAsync(
        ImageGenerationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Mirror of the JS SDK: openrouter.chat.send({ model, messages, modalities })
        var payload = new OpenRouterRequest
        {
            Model = _config.Model,
            Messages = [new() { Role = "user", Content = request.Prompt }],
            Modalities = ["image"]
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = content
        };

        // Auth — set per-request, not in constructor
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _config.ApiKey);

        _logger.LogInformation(
            "OpenRouter image generation started. Model={Model}, Prompt={Prompt}",
            _config.Model, request.Prompt);

        var response = await _http.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "OpenRouter {StatusCode}: {Body}",
                (int)response.StatusCode, errorBody);
            response.EnsureSuccessStatusCode();
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogDebug("OpenRouter raw response: {Raw}", raw);

        return ParseResponse(raw);
    }

    // -------------------------------------------------------------------------
    // Parsing — mirrors JS: result.choices[0].message.images[].image_url.url
    // -------------------------------------------------------------------------

    private static IReadOnlyList<ImageGenerationResult> ParseResponse(string raw)
    {
        var root = JsonSerializer.Deserialize<OpenRouterResponse>(raw, JsonOptions)
            ?? throw new InvalidOperationException("Empty response from OpenRouter.");

        var images = new List<ImageGenerationResult>();

        foreach (var choice in root.Choices ?? [])
        {
            foreach (var image in choice.Message?.Images ?? [])
            {
                var url = image.ImageUrl?.Url;
                if (!string.IsNullOrWhiteSpace(url))
                    images.Add(new ImageGenerationResult { DataUrl = url });
            }
        }

        return images;
    }

    private sealed class OpenRouterRequest
    {
        public string Model { get; set; } = default!;
        public List<Message> Messages { get; set; } = [];
        public List<string> Modalities { get; set; } = [];
    }

    private sealed class Message
    {
        public string Role { get; set; } = default!;
        public string Content { get; set; } = default!;
    }

    // result.choices[]
    private sealed class OpenRouterResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    // result.choices[i]
    private sealed class Choice
    {
        public AssistantMessage? Message { get; set; }
    }

    // result.choices[i].message
    private sealed class AssistantMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public List<Image>? Images { get; set; }  // message.images[]
    }

    // result.choices[i].message.images[j]
    private sealed class Image
    {
        public ImageUrl? ImageUrl { get; set; }     // image.image_url
    }

    // result.choices[i].message.images[j].image_url
    private sealed class ImageUrl
    {
        public string? Url { get; set; }            // image.image_url.url
    }
}
