using System.Text.Json;
using AI.Application.Features.Post.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Marketing.Application.Services;
using Marketing.Infrastructure.Configs;

namespace Marketing.Infrastructure.Services;

public sealed class ThreadsMetricsService : IThreadsMetricsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ThreadsConfig _config;
    private readonly ILogger<ThreadsMetricsService> _logger;

    public ThreadsMetricsService(
        IHttpClientFactory httpClientFactory,
        IOptions<ThreadsConfig> config,
        ILogger<ThreadsMetricsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<ThreadsMetricsDto?> GetMetricsAsync(
        string mediaId,
        string? externalUrl,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Threads");

            var metrics = "likes,replies,reposts,quotes,shares,views";
            var url = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{mediaId}/insights" +
                      $"?metric={Uri.EscapeDataString(metrics)}&access_token={_config.AccessToken}";

            var response = await client.GetAsync(url, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Threads media insights returned {Status} for media {MediaId}: {Error}",
                    response.StatusCode, mediaId, raw);
                return null;
            }

            using var document = JsonDocument.Parse(raw);
            var json = document.RootElement;

            var likes = ExtractMetric(json, "likes");
            var replies = ExtractMetric(json, "replies");
            var reposts = ExtractMetric(json, "reposts");
            var quotes = ExtractMetric(json, "quotes");
            var shares = ExtractMetric(json, "shares");
            var views = ExtractMetric(json, "views");

            var engagementTotal = likes + replies + reposts + quotes + shares;
            var engagementRate = views > 0
                ? Math.Round((double)engagementTotal / views * 100, 2)
                : 0;

            return new ThreadsMetricsDto
            {
                ExternalPostId = mediaId,
                ExternalUrl = externalUrl ?? string.Empty,
                Likes = (int)likes,
                Replies = (int)replies,
                Reposts = (int)reposts,
                Quotes = (int)quotes,
                Shares = (int)shares,
                Views = views,
                EngagementRate = engagementRate,
                EngagementRateFormatted = $"{engagementRate}%",
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Threads metrics for media {MediaId}", mediaId);
            return null;
        }
    }

    public async Task<ThreadsAccountMetricsDto?> GetAccountTotalsAsync(
        string? since = null,
        string? until = null,
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Threads");

            var sinceValue = since ?? DateTimeOffset.UtcNow.AddDays(-28).ToUnixTimeSeconds().ToString();
            var untilValue = until ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var metrics = "views,likes,replies,reposts,quotes";
            var insightsUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{_config.UserId}/threads_insights" +
                              $"?metric={Uri.EscapeDataString(metrics)}" +
                              $"&metric_type=total_value" +
                              $"&since={sinceValue}&until={untilValue}" +
                              $"&access_token={_config.AccessToken}";

            var userUrl = $"{_config.GraphApiBaseUrl}/{_config.GraphApiVersion}/{_config.UserId}" +
                          $"?fields=followers_count&access_token={_config.AccessToken}";

            var insightsTask = client.GetAsync(insightsUrl, ct);
            var userTask = client.GetAsync(userUrl, ct);

            await Task.WhenAll(insightsTask, userTask);

            var insightsResponse = await insightsTask;
            var userResponse = await userTask;

            var insightsRaw = await insightsResponse.Content.ReadAsStringAsync(ct);
            var userRaw = await userResponse.Content.ReadAsStringAsync(ct);

            if (!insightsResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Threads account insights returned {Status} for user {UserId}: {Error}",
                    insightsResponse.StatusCode, _config.UserId, insightsRaw);
                return null;
            }

            long followersCount = 0;
            if (userResponse.IsSuccessStatusCode)
            {
                using var userDocument = JsonDocument.Parse(userRaw);
                if (userDocument.RootElement.TryGetProperty("followers_count", out var followersElement))
                {
                    followersCount = followersElement.GetInt64();
                }
            }
            else
            {
                _logger.LogWarning("Threads followers lookup returned {Status} for user {UserId}: {Error}",
                    userResponse.StatusCode, _config.UserId, userRaw);
            }

            using var insightsDocument = JsonDocument.Parse(insightsRaw);
            var json = insightsDocument.RootElement;

            return new ThreadsAccountMetricsDto
            {
                UserId = _config.UserId,
                Views = ExtractMetric(json, "views"),
                Likes = (int)ExtractMetric(json, "likes"),
                Replies = (int)ExtractMetric(json, "replies"),
                Reposts = (int)ExtractMetric(json, "reposts"),
                Quotes = (int)ExtractMetric(json, "quotes"),
                FollowersCount = followersCount,
                FetchedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Threads account totals");
            return null;
        }
    }

    private static long ExtractMetric(JsonElement json, string metricName)
    {
        try
        {
            if (!json.TryGetProperty("data", out var data))
                return 0;

            var node = data.EnumerateArray()
                .FirstOrDefault(metric =>
                    metric.TryGetProperty("name", out var name) &&
                    name.GetString() == metricName);

            if (node.ValueKind == JsonValueKind.Undefined)
                return 0;

            if (node.TryGetProperty("total_value", out var totalValue) &&
                totalValue.TryGetProperty("value", out var totalValueValue) &&
                totalValueValue.ValueKind == JsonValueKind.Number)
            {
                return totalValueValue.GetInt64();
            }

            if (node.TryGetProperty("values", out var values))
            {
                var lastEntry = values.EnumerateArray().LastOrDefault();
                if (lastEntry.ValueKind != JsonValueKind.Undefined &&
                    lastEntry.TryGetProperty("value", out var value) &&
                    value.ValueKind == JsonValueKind.Number)
                {
                    return value.GetInt64();
                }
            }
        }
        catch
        {
            return 0;
        }

        return 0;
    }
}