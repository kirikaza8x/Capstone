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

            var pageToken = await GetPageAccessTokenAsync(client, ct);
            if (pageToken is null)
            {
                _logger.LogWarning("Could not retrieve page access token for post {PostId}", externalPostId);
                return null;
            }

            var fields = "likes.summary(true),comments.summary(true),shares";
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}" +
                      $"?fields={fields}&access_token={pageToken}";

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

    private async Task<string?> GetPageAccessTokenAsync(HttpClient client, CancellationToken ct)
    {
        try
        {
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/me/accounts" +
                      $"?access_token={_config.PageAccessToken}";

            var response = await client.GetAsync(url, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("Page accounts response: {Response}", raw);

            if (!response.IsSuccessStatusCode) return null;

            var json = JsonSerializer.Deserialize<JsonElement>(raw);

            var page = json.GetProperty("data")
                .EnumerateArray()
                .FirstOrDefault(p => p.GetProperty("id").GetString() == _config.PageId);

            return page.ValueKind != JsonValueKind.Undefined
                ? page.GetProperty("access_token").GetString()
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve page access token");
            return null;
        }
    }
}