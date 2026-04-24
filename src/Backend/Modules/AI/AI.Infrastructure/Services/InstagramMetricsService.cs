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

    // -------------------------------------------------------------------------
    // POST METRICS
    // -------------------------------------------------------------------------

    public async Task<InstagramMetricsDto?> GetMetricsAsync(
        string externalPostId,
        string externalUrl,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Instagram");
            var token = _config.PageAccessToken;

            // Media-level insights still use the legacy fields= approach on the media node.
            // likes/comments/reach/saved are lifetime metrics on individual media objects.
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}" +
                      $"?fields=insights.metric(impressions,reach,likes,comments,saved,shares)" +
                      $"&access_token={token}";

            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Instagram media insights returned {Status} for post {PostId}: {Error}",
                    response.StatusCode, externalPostId, error);
                return null;
            }

            var raw = await response.Content.ReadAsStringAsync(ct);
            var json = JsonDocument.Parse(raw).RootElement;

            return new InstagramMetricsDto
            {
                ExternalPostId = externalPostId,
                ExternalUrl = externalUrl,
                Likes = (int)ExtractMediaInsightValue(json, "likes"),
                Comments = (int)ExtractMediaInsightValue(json, "comments"),
                Reach = ExtractMediaInsightValue(json, "reach"),
                Saves = ExtractMediaInsightValue(json, "saved"),
                Shares = ExtractMediaInsightValue(json, "shares"),
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Instagram post metrics for post {PostId}", externalPostId);
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // PAGE / ACCOUNT METRICS
    // -------------------------------------------------------------------------

    public async Task<InstagramPageMetricsDto?> GetPageTotalsAsync(
        InstagramPeriod period = InstagramPeriod.days_28,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Instagram");
            var token = _config.PageAccessToken;
            var accountId = _config.AccountId;

            // Instagram's new insights API uses Unix timestamp ranges instead of period strings.
            // period=day still tells the API the granularity, but since/until define the window.
            var until = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var since = period switch
            {
                InstagramPeriod.day => DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
                InstagramPeriod.week => DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds(),
                InstagramPeriod.days_28 => DateTimeOffset.UtcNow.AddDays(-28).ToUnixTimeSeconds(),
                _ => DateTimeOffset.UtcNow.AddDays(-28).ToUnixTimeSeconds()
            };

            var baseInsightsUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}/insights";

            // --- Request 1: Core interaction metrics ---
            // reach, views (replaces deprecated impressions), likes, comments,
            // shares, saves, reposts, total_interactions, accounts_engaged
            var interactionMetrics = Uri.EscapeDataString(
                "reach,views,likes,comments,shares,saves,reposts,total_interactions,accounts_engaged");

            var interactionUrl = $"{baseInsightsUrl}" +
                                 $"?metric={interactionMetrics}" +
                                 $"&period=day" +
                                 $"&metric_type=total_value" +
                                 $"&since={since}&until={until}" +
                                 $"&access_token={token}";

            // --- Request 2: follows_and_unfollows ---
            // Requires breakdown=follow_type to separate follows gained vs lost.
            // Not returned for accounts with fewer than 100 followers.
            var followUrl = $"{baseInsightsUrl}" +
                            $"?metric=follows_and_unfollows" +
                            $"&period=day" +
                            $"&metric_type=total_value" +
                            $"&breakdown=follow_type" +
                            $"&since={since}&until={until}" +
                            $"&access_token={token}";

            // --- Request 3: Follower count from user node ---
            var userUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}" +
                          $"?fields=followers_count&access_token={token}";

            var interactionTask = client.GetAsync(interactionUrl, ct);
            var followTask = client.GetAsync(followUrl, ct);
            var userTask = client.GetAsync(userUrl, ct);

            await Task.WhenAll(interactionTask, followTask, userTask);

            var interactionResponse = await interactionTask;
            var followResponse = await followTask;
            var userResponse = await userTask;

            var interactionRaw = await interactionResponse.Content.ReadAsStringAsync(ct);
            var followRaw = await followResponse.Content.ReadAsStringAsync(ct);
            var userRaw = await userResponse.Content.ReadAsStringAsync(ct);

            _logger.LogDebug("Instagram interaction insights raw for account {AccountId}: {Raw}",
                accountId, interactionRaw);
            _logger.LogDebug("Instagram follow insights raw for account {AccountId}: {Raw}",
                accountId, followRaw);

            JsonElement interactionJson = default;
            JsonElement followJson = default;
            var userJson = JsonDocument.Parse(userRaw).RootElement;

            if (interactionResponse.IsSuccessStatusCode)
            {
                interactionJson = JsonDocument.Parse(interactionRaw).RootElement;
            }
            else
            {
                _logger.LogWarning("Instagram interaction insights returned {Status} for account {AccountId}: {Error}",
                    interactionResponse.StatusCode, accountId, interactionRaw);
            }

            if (followResponse.IsSuccessStatusCode)
            {
                followJson = JsonDocument.Parse(followRaw).RootElement;
            }
            else
            {
                _logger.LogWarning("Instagram follow insights returned {Status} for account {AccountId}: {Error}",
                    followResponse.StatusCode, accountId, followRaw);
            }

            long followers = userJson.TryGetProperty("followers_count", out var fc)
                ? fc.GetInt64() : 0;

            var (follows, unfollows) = followJson.ValueKind != JsonValueKind.Undefined
                ? ExtractFollowBreakdown(followJson)
                : (0L, 0L);

            return new InstagramPageMetricsDto
            {
                AccountId = accountId,
                AccountUrl = $"https://instagram.com/{accountId}",
                Period = period,
                FollowersCount = followers,
                Follows = follows,
                Unfollows = unfollows,
                Reach = ExtractTotalValue(interactionJson, "reach"),
                Views = ExtractTotalValue(interactionJson, "views"),
                Likes = ExtractTotalValue(interactionJson, "likes"),
                Comments = ExtractTotalValue(interactionJson, "comments"),
                Shares = ExtractTotalValue(interactionJson, "shares"),
                Saves = ExtractTotalValue(interactionJson, "saves"),
                Reposts = ExtractTotalValue(interactionJson, "reposts"),
                TotalInteractions = ExtractTotalValue(interactionJson, "total_interactions"),
                AccountsEngaged = ExtractTotalValue(interactionJson, "accounts_engaged"),
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Instagram page totals for period {Period}", period);
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // PRIVATE HELPERS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Extracts a metric's total_value from the new Instagram Account Insights API shape.
    /// New shape: data[] -> { name, total_value: { value } }
    /// This replaces the old values[] time-series extraction.
    /// </summary>
    private long ExtractTotalValue(JsonElement json, string metricName)
    {
        try
        {
            if (json.ValueKind == JsonValueKind.Undefined) return 0;
            if (!json.TryGetProperty("data", out var data)) return 0;

            var node = data.EnumerateArray()
                .FirstOrDefault(m =>
                    m.TryGetProperty("name", out var n) &&
                    n.GetString() == metricName);

            if (node.ValueKind == JsonValueKind.Undefined) return 0;

            if (node.TryGetProperty("total_value", out var tv) &&
                tv.TryGetProperty("value", out var val))
            {
                return val.GetInt64();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("ExtractTotalValue failed for metric {MetricName}: {Message}",
                metricName, ex.Message);
        }

        return 0;
    }

    /// <summary>
    /// Extracts follows and unfollows from the follows_and_unfollows metric.
    /// The breakdown by follow_type splits: FOLLOWER (gained) vs UNFOLLOWER (lost).
    /// Shape: data[] -> { name, total_value: { breakdowns[]: { results[]: { dimension_values[], value } } } }
    /// </summary>
    private (long follows, long unfollows) ExtractFollowBreakdown(JsonElement json)
    {
        long follows = 0, unfollows = 0;

        try
        {
            if (!json.TryGetProperty("data", out var data)) return (0, 0);

            var node = data.EnumerateArray()
                .FirstOrDefault(m =>
                    m.TryGetProperty("name", out var n) &&
                    n.GetString() == "follows_and_unfollows");

            if (node.ValueKind == JsonValueKind.Undefined) return (0, 0);
            if (!node.TryGetProperty("total_value", out var tv)) return (0, 0);
            if (!tv.TryGetProperty("breakdowns", out var breakdowns)) return (0, 0);

            foreach (var breakdown in breakdowns.EnumerateArray())
            {
                if (!breakdown.TryGetProperty("results", out var results)) continue;

                foreach (var result in results.EnumerateArray())
                {
                    if (!result.TryGetProperty("dimension_values", out var dims)) continue;
                    if (!result.TryGetProperty("value", out var val)) continue;

                    var followType = dims.EnumerateArray().FirstOrDefault().GetString();

                    if (followType == "FOLLOWER") follows = val.GetInt64();
                    if (followType == "UNFOLLOWER") unfollows = val.GetInt64();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("ExtractFollowBreakdown failed: {Message}", ex.Message);
        }

        return (follows, unfollows);
    }

    /// <summary>
    /// Extracts a metric value from an individual media object's insights.
    /// Handles both legacy shapes: total_value.value and values[].value
    /// Used by GetMetricsAsync (post-level), not page-level insights.
    /// </summary>
    private long ExtractMediaInsightValue(JsonElement json, string metricName)
    {
        try
        {
            JsonElement dataArray;

            // The media insights endpoint nests under insights.data
            if (json.TryGetProperty("insights", out var insights) &&
                insights.TryGetProperty("data", out dataArray)) { }
            else if (json.TryGetProperty("data", out dataArray)) { }
            else return 0;

            var node = dataArray.EnumerateArray()
                .FirstOrDefault(m =>
                    m.TryGetProperty("name", out var n) &&
                    n.GetString() == metricName);

            if (node.ValueKind == JsonValueKind.Undefined) return 0;

            // Prefer total_value shape (newer)
            if (node.TryGetProperty("total_value", out var tv) &&
                tv.TryGetProperty("value", out var tvVal))
            {
                return tvVal.GetInt64();
            }

            // Fall back to values[] time-series shape (legacy media insights)
            if (node.TryGetProperty("values", out var values) &&
                values.GetArrayLength() > 0)
            {
                return values.EnumerateArray()
                    .Last()
                    .TryGetProperty("value", out var v) ? v.GetInt64() : 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("ExtractMediaInsightValue failed for metric {MetricName}: {Message}",
                metricName, ex.Message);
        }

        return 0;
    }
}