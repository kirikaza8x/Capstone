using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Services;
using Marketing.Infrastructure.Configs;
using Marketing.Domain.Enums;

namespace Marketing.Infrastructure.Services;

public sealed class InstagramMetricsService : IInstagramMetricsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InstagramConfig _config;
    private readonly ILogger<InstagramMetricsService> _logger;

    public InstagramMetricsService(
        IHttpClientFactory httpClientFactory,
        IOptions<InstagramConfig> config,
        ILogger<InstagramMetricsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<InstagramMetricsDto?> GetMetricsAsync(
        string externalPostId,
        string externalUrl,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Instagram");
            var token = _config.PageAccessToken;

            var metrics = "impressions,reach,likes,comments,saved";
            
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}" +
                      $"?fields=insights.metric({metrics})&access_token={token}";

            _logger.LogInformation("Fetching Instagram post metrics: {Url}", url);

            var response = await client.GetAsync(url, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("Instagram Post API response ({Status}): {Raw}", 
                response.StatusCode, raw);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Instagram Post API returned {Status} for post {PostId}: {Error}",
                    response.StatusCode, externalPostId, raw);
                return null;
            }

            var json = JsonDocument.Parse(raw).RootElement;

            var likes = ExtractInsightValue(json, "likes");
            var comments = ExtractInsightValue(json, "comments");
            var reach = ExtractInsightValue(json, "reach");
            var saves = ExtractInsightValue(json, "saved");

            _logger.LogInformation("Extracted post metrics - Likes: {Likes}, Comments: {Comments}, Reach: {Reach}, Saves: {Saves}",
                likes, comments, reach, saves);

            return new InstagramMetricsDto
            {
                ExternalPostId = externalPostId,
                ExternalUrl = externalUrl,
                Likes = (int)likes,           
                Comments = (int)comments,     
                Reach = reach,               
                Saves = saves,               
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Instagram metrics for post {PostId}", externalPostId);
            return null;
        }
    }

    public async Task<InstagramPageMetricsDto?> GetPageTotalsAsync(
        InstagramPeriod period = InstagramPeriod.days_28,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Instagram");
            var token = _config.PageAccessToken;
            var accountId = _config.AccountId;

            // ✅ VALID Instagram User insights metrics ONLY (no follower_count)
            var metrics = "reach,profile_views,total_interactions,website_clicks";
            
            var periodString = period switch
            {
                InstagramPeriod.week => "week",
                InstagramPeriod.day => "day",
                InstagramPeriod.days_28 => "days_28",
                InstagramPeriod.month => "month",
                InstagramPeriod.lifetime => "lifetime",
                _ => "days_28"
            };

            // ✅ Page insights: use /insights with metric_type=total_value
            var insightsUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}/insights" +
                              $"?metric={Uri.EscapeDataString(metrics)}" +
                              $"&period={periodString}" +
                              $"&metric_type=total_value" +
                              $"&access_token={token}";

            _logger.LogInformation("Fetching Instagram page insights: {Url}", insightsUrl);

            var response = await client.GetAsync(insightsUrl, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("Instagram Insights response ({Status}): {Raw}", 
                response.StatusCode, raw);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Instagram Insights {Status} for period {Period}: {Error}",
                    response.StatusCode, periodString, raw);
                return null;
            }

            var json = JsonDocument.Parse(raw).RootElement;

            // ✅ Follower count: fetch from direct User endpoint (NOT /insights)
            var followerUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}" +
                              $"?fields=follower_count&access_token={token}";

            _logger.LogInformation("Fetching follower_count from User endpoint: {Url}", followerUrl);

            var followerResponse = await client.GetAsync(followerUrl, ct);
            var followerRaw = await followerResponse.Content.ReadAsStringAsync(ct);
            _logger.LogInformation("Follower response ({Status}): {Raw}", followerResponse.StatusCode, followerRaw);

            long followers = 0;
            if (followerResponse.IsSuccessStatusCode)
            {
                var followerJson = await followerResponse.Content.ReadFromJsonAsync<JsonElement>(ct);
                followers = followerJson.TryGetProperty("follower_count", out var fc) && fc.ValueKind == JsonValueKind.Number
                    ? fc.GetInt64()
                    : 0;
                
                _logger.LogInformation("Extracted follower_count = {Followers} (from User endpoint)", followers);
            }

            var reach = ExtractInsightValue(json, "reach");
            var profileViews = ExtractInsightValue(json, "profile_views");
            var engagement = ExtractInsightValue(json, "total_interactions");
            var websiteClicks = ExtractInsightValue(json, "website_clicks");

            _logger.LogInformation("Extracted page metrics - Followers: {Followers}, Reach: {Reach}, ProfileViews: {ProfileViews}, Engagement: {Engagement}, WebsiteClicks: {WebsiteClicks}",
                followers, reach, profileViews, engagement, websiteClicks);

            return new InstagramPageMetricsDto
            {
                AccountId = _config.AccountId,
                AccountUrl = $"https://instagram.com/{_config.AccountId}",
                Period = period,
                FollowersCount = followers,
                Reach = reach,
                ProfileViews = profileViews,
                Engagement = engagement,
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Instagram page totals for period {Period}", period);
            return null;
        }
    }

    /// <summary>
    /// Extracts a single metric value from Instagram Insights JSON.
    /// Handles BOTH response structures:
    /// - metric_type=total_value: data[].total_value.value
    /// - default period-based: data[].values[].value OR insights.data[].values[].value
    /// </summary>
    private long ExtractInsightValue(JsonElement json, string metricName)
    {
        try
        {
            // ✅ Structure A: { "data": [ { "name": "...", "total_value": { "value": 123 } } ] }
            if (json.TryGetProperty("data", out var dataArray))
            {
                var metricNode = dataArray.EnumerateArray()
                    .FirstOrDefault(m => m.TryGetProperty("name", out var name) && name.GetString() == metricName);

                if (metricNode.ValueKind != JsonValueKind.Undefined)
                {
                    // Try total_value (for metric_type=total_value)
                    if (metricNode.TryGetProperty("total_value", out var totalVal) &&
                        totalVal.TryGetProperty("value", out var val) &&
                        val.ValueKind == JsonValueKind.Number)
                    {
                        _logger.LogInformation("Extracted {MetricName} = {Value} (from total_value)", metricName, val.GetInt64());
                        return val.GetInt64();
                    }

                    // Try values[] (for period-based responses without metric_type)
                    if (metricNode.TryGetProperty("values", out var values) && values.EnumerateArray().Any())
                    {
                        var lastValue = values.EnumerateArray().Last();
                        if (lastValue.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.Number)
                        {
                            _logger.LogInformation("Extracted {MetricName} = {Value} (from values[])", metricName, v.GetInt64());
                            return v.GetInt64();
                        }
                    }
                }
            }

            // ✅ Structure B (fallback): { "insights": { "data": [...] } }
            if (json.TryGetProperty("insights", out var insights) &&
                insights.TryGetProperty("data", out var data))
            {
                var metricNode = data.EnumerateArray()
                    .FirstOrDefault(m => m.TryGetProperty("name", out var name) && name.GetString() == metricName);

                if (metricNode.ValueKind != JsonValueKind.Undefined &&
                    metricNode.TryGetProperty("values", out var values) &&
                    values.EnumerateArray().Any())
                {
                    var lastValue = values.EnumerateArray().Last();
                    if (lastValue.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Number)
                    {
                        _logger.LogInformation("Extracted {MetricName} = {Value} (from insights.values[])", metricName, val.GetInt64());
                        return val.GetInt64();
                    }
                }
            }

            _logger.LogInformation("Metric {MetricName} not found or has no value", metricName);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "JSON extraction failed for Instagram metric {MetricName}", metricName);
            return 0;
        }
    }
}