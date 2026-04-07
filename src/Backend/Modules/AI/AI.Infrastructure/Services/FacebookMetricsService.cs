using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Services;
using Marketing.Infrastructure.Configs;
using Marketing.Domain.Enums;

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

    public async Task<FacebookPageMetricsDto?> GetPageTotalsAsync(FacebookPeriod period = FacebookPeriod.days_28, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Facebook");
            var pageToken = await GetPageAccessTokenAsync(client, ct);
            if (pageToken is null) return null;

            var metrics = "page_daily_unfollows_unique,page_daily_follows_unique,page_views_total,page_impressions_unique,page_actions_post_reactions_like_total,page_post_engagements";

            var periodString = period.ToString().ToLower();

            var baseUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{_config.PageId}/insights";
            var queryParams = $"?metric={Uri.EscapeDataString(metrics)}&period={periodString}&access_token={pageToken}";
            var url = baseUrl + queryParams;

            _logger.LogDebug("Fetching Facebook insights for period {Period}: {Url}", periodString, url);

            var response = await client.GetAsync(url, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Facebook Insights {Status} for period {Period}: {Error}", response.StatusCode, periodString, raw);
                return null;
            }

            var json = JsonDocument.Parse(raw).RootElement;

            return new FacebookPageMetricsDto
            {
                PageId = _config.PageId,
                PageUrl = $"https://facebook.com/{_config.PageId}",
                Period = period, 
                DailyUnfollowsUnique = ExtractMetric(json, "page_daily_unfollows_unique"),
                DailyFollowsUnique = ExtractMetric(json, "page_daily_follows_unique"),
                ViewsTotal = ExtractMetric(json, "page_views_total"),
                ImpressionsUnique = ExtractMetric(json, "page_impressions_unique"),
                LikesTotal = ExtractMetric(json, "page_actions_post_reactions_like_total"),
                PostEngagements = ExtractMetric(json, "page_post_engagements"),

                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Facebook page totals for period {Period}", period);
            return null;
        }
    }

    /// <summary>
    /// Safely extracts the metric value from the Facebook Insights JSON structure.
    /// Navigates through: data[] -> values[] -> value
    /// </summary>
    private long ExtractMetric(JsonElement json, string metricName)
    {
        try
        {
            if (!json.TryGetProperty("data", out var data)) return 0;

            // Find the specific metric object in the data array
            var metricNode = data.EnumerateArray()
                .FirstOrDefault(m => m.TryGetProperty("name", out var name) && name.GetString() == metricName);

            // Ensure the metric exists and has a 'values' array
            if (metricNode.ValueKind != JsonValueKind.Undefined && metricNode.TryGetProperty("values", out var values))
            {
                var valuesList = values.EnumerateArray().ToList();

                if (valuesList.Any())
                {
                    // The API returns a time-series. We take the last entry for the most recent 28-day snapshot.
                    var lastEntry = valuesList.Last();

                    if (lastEntry.TryGetProperty("value", out var val))
                    {
                        return val.ValueKind == JsonValueKind.Number ? val.GetInt64() : 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("JSON extraction failed for metric {MetricName}: {Message}", metricName, ex.Message);
        }

        return 0;
    }

    public async Task<string?> GetPageAccessTokenAsync(HttpClient client, CancellationToken ct)
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