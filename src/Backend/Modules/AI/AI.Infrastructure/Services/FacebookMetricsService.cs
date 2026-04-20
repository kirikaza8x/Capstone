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
    /// <summary>
    /// 
    /// </summary>
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
            // 1. PUBLIC ENGAGEMENT LOOKUP (Likes, Comments, and Shares)
            // Note: We use the URL endpoint because the Post ID endpoint often returns 
            // empty objects for shares due to "Page Public Content Access" restrictions.
            var fields = "likes.summary(true),comments.summary(true)";
            var postUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}" +
                          $"?fields={fields}&access_token={pageToken}";

            var postResponse = await client.GetAsync(postUrl, ct);

            if (!postResponse.IsSuccessStatusCode)
            {
                var error = await postResponse.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Facebook Post API returned {Status} for post {PostId}: {Error}",
                    postResponse.StatusCode, externalPostId, error);
                return null;
            }

            var postJson = await postResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            var metrics = "post_clicks,post_impressions_unique";
            var insightsUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}/insights" +
                              $"?metric={Uri.EscapeDataString(metrics)}&access_token={pageToken}";

            var insightsResponse = await client.GetAsync(insightsUrl, ct);
            var insightsRaw = await insightsResponse.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("Facebook insights raw response for post {PostId}: {Raw}",
                externalPostId, insightsRaw);

            long reach = 0;
            long clicks = 0;

            if (insightsResponse.IsSuccessStatusCode)
            {
                var insightsJson = JsonDocument.Parse(insightsRaw).RootElement;

                reach = ExtractMetric(insightsJson, "post_impressions_unique");
                clicks = ExtractMetric(insightsJson, "post_clicks");
            }
            else
            {
                _logger.LogWarning("Facebook Insights API returned {Status} for post {PostId}: {Error}",
                    insightsResponse.StatusCode, externalPostId, insightsRaw);
            }

            return new FacebookMetricsDto
            {
                ExternalPostId = externalPostId,
                ExternalUrl = externalUrl,
                Likes = postJson.TryGetProperty("likes", out var likes)
                    && likes.TryGetProperty("summary", out var likesSummary)
                    && likesSummary.TryGetProperty("total_count", out var likesCount)
                        ? likesCount.GetInt32()
                        : 0,

                Comments = postJson.TryGetProperty("comments", out var comments)
                    && comments.TryGetProperty("summary", out var commentsSummary)
                    && commentsSummary.TryGetProperty("total_count", out var commentsCount)
                        ? commentsCount.GetInt32()
                        : 0,
                Reach = reach,
                Clicks = clicks,
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