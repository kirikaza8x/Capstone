using AI.Application.Features.Post.DTOs;

namespace Marketing.Application.Services;

public interface IThreadsMetricsService
{
    Task<ThreadsMetricsDto?> GetMetricsAsync(
        string mediaId,
        string? externalUrl,
        CancellationToken ct = default);

    Task<ThreadsAccountMetricsDto?> GetAccountTotalsAsync(
        string? since = null,
        string? until = null,
        CancellationToken ct = default);
}