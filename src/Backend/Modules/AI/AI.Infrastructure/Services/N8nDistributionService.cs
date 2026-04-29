using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Marketing.Application.Services;
using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Marketing.Infrastructure.Configs;

namespace Marketing.Infrastructure.Services;

public class N8nDistributionService : IN8nDistributionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nIntegrationConfig _config;
    private readonly ILogger<N8nDistributionService> _logger;

    public N8nDistributionService(
        IHttpClientFactory httpClientFactory,
        IOptions<N8nIntegrationConfig> config,
        ILogger<N8nDistributionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        PostMarketing post,
        ExternalPlatform platform,
        CancellationToken ct = default)
    {
        var cleanTitle = GeminiTextStripper.StripHtml(post.Title);
        var cleanSummary = GeminiTextStripper.StripHtml(post.Summary);
        var cleanBody = GeminiTextStripper.BodyBlocksToPlainText(post.Body, platform, post.Id);

        var publicUrl = $"{_config.AppBaseUrl}/posts/{post.Slug}";

        var payload = new
        {
            post_id = post.Id,
            tracking_token = post.TrackingToken,
            title = cleanTitle,
            summary = cleanSummary,
            body = cleanBody,
            image_url = post.ImageUrl,
            platform = platform.ToString(),
            public_url = publicUrl,
            queued_at = DateTime.UtcNow
        };

        try
        {
            var client = _httpClientFactory.CreateClient("N8n");

            var response = await client.PostAsJsonAsync(
                string.Empty,  // BaseAddress is the full webhook URL from config
                payload,
                ct);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP call to n8n failed for Post {PostId}", post.Id);
            return false;
        }
    }
}
