using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Services;
using Marketing.Infrastructure.Configs;

namespace Marketing.Infrastructure.Services;

public sealed class FacebookMetricsService : IFacebookMetricsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FacebookConfig _config;
    private readonly ILogger<FacebookMetricsService> _logger;

    public FacebookMetricsService(
        IHttpClientFactory httpClientFactory,
        IOptions<FacebookConfig> config,
        ILogger<FacebookMetricsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<FacebookMetricsDto?> GetMetricsAsync(
        string externalPostId,
        string externalUrl,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Facebook");

            var fields = "likes.summary(true),comments.summary(true),shares";
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}" +
                      $"?fields={fields}&access_token={_config.PageAccessToken}";

            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Facebook API returned {Status} for post {PostId}: {Error}",
                    response.StatusCode, externalPostId, error);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

            return new FacebookMetricsDto
            {
                ExternalPostId = externalPostId,
                ExternalUrl = externalUrl,
                Likes = json.TryGetProperty("likes", out var likes)
                    ? likes.GetProperty("summary").GetProperty("total_count").GetInt32()
                    : 0,
                Comments = json.TryGetProperty("comments", out var comments)
                    ? comments.GetProperty("summary").GetProperty("total_count").GetInt32()
                    : 0,
                Shares = json.TryGetProperty("shares", out var shares)
                    ? shares.GetProperty("count").GetInt32()
                    : 0,
                Impressions = 0,
                Reach = 0,
                Clicks = 0,
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Facebook metrics for post {PostId}", externalPostId);
            return null;
        }
    }
}