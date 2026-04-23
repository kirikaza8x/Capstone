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
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{externalPostId}?fields=insights.metric(impressions,reach,likes,comments,saved)&access_token={token}";

            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            var raw = await response.Content.ReadAsStringAsync(ct);
            var json = JsonDocument.Parse(raw).RootElement;

            return new InstagramMetricsDto
            {
                ExternalPostId = externalPostId,
                ExternalUrl = externalUrl,
                Likes = (int)ExtractInsightValue(json, "likes"),
                Comments = (int)ExtractInsightValue(json, "comments"),
                Reach = ExtractInsightValue(json, "reach"),
                Saves = ExtractInsightValue(json, "saved"),
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch post metrics {Id}", externalPostId);
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

            int daysCount = period switch
            {
                InstagramPeriod.day => 1,
                InstagramPeriod.week => 7,
                InstagramPeriod.days_28 => 28,
                _ => 1
            };

            var periodString = (period == InstagramPeriod.day) ? "day" :
                               (period == InstagramPeriod.week) ? "week" : "days_28";

            var reachUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}/insights?metric=reach&period={periodString}&access_token={token}";

            var interUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}/insights?metric=likes,comments,total_interactions&period=day&metric_type=total_value&access_token={token}";

            var userUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{accountId}?fields=followers_count&access_token={token}";

            var reachTask = client.GetAsync(reachUrl, ct);
            var interTask = client.GetAsync(interUrl, ct);
            var userTask = client.GetAsync(userUrl, ct);

            await Task.WhenAll(reachTask, interTask, userTask);

            var reachRaw = await (await reachTask).Content.ReadAsStringAsync(ct);
            var interRaw = await (await interTask).Content.ReadAsStringAsync(ct);
            var userRaw = await (await userTask).Content.ReadAsStringAsync(ct);

            var reachJson = JsonDocument.Parse(reachRaw).RootElement;
            var interJson = JsonDocument.Parse(interRaw).RootElement;
            var userJson = JsonDocument.Parse(userRaw).RootElement;

            long followers = userJson.TryGetProperty("followers_count", out var fc) ? fc.GetInt64() : 0;

            return new InstagramPageMetricsDto
            {
                AccountId = accountId,
                AccountUrl = $"https://instagram.com/{accountId}", 
                Period = period,
                FollowersCount = followers,
                Reach = ExtractInsightValue(reachJson, "reach"),
                Engagement = SumMetricValues(interJson, "total_interactions", daysCount), 
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Failed to fetch page totals");
            return null;
        }
    }

    private long ExtractInsightValue(JsonElement json, string metricName)
    {
        try
        {
            JsonElement dataArray;
            if (json.TryGetProperty("data", out dataArray)) { }
            else if (json.TryGetProperty("insights", out var insights) && insights.TryGetProperty("data", out dataArray)) { }
            else return 0;

            var metricNode = dataArray.EnumerateArray().FirstOrDefault(m => m.GetProperty("name").GetString() == metricName);
            if (metricNode.ValueKind == JsonValueKind.Undefined) return 0;

            if (metricNode.TryGetProperty("total_value", out var tv)) return tv.GetProperty("value").GetInt64();

            if (metricNode.TryGetProperty("values", out var vs) && vs.GetArrayLength() > 0)
                return vs.EnumerateArray().Last().GetProperty("value").GetInt64();

            return 0;
        }
        catch { return 0; }
    }

    private long SumMetricValues(JsonElement json, string metricName, int days)
    {
        try
        {
            if (!json.TryGetProperty("data", out var dataArray)) return 0;

            var metricNode = dataArray.EnumerateArray()
                .FirstOrDefault(m => m.GetProperty("name").GetString() == metricName);

            if (metricNode.ValueKind == JsonValueKind.Undefined) return 0;

            if (metricNode.TryGetProperty("total_value", out var tv))
            {
                return tv.GetProperty("value").GetInt64();
            }

            if (metricNode.TryGetProperty("values", out var values) && values.GetArrayLength() > 0)
            {
                return values.EnumerateArray()
                    .TakeLast(days)
                    .Sum(v => v.TryGetProperty("value", out var val) ? val.GetInt64() : 0);
            }

            return 0;
        }
        catch { return 0; }
    }

}
