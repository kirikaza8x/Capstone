using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;

namespace Marketing.Application.Services;

public interface IInstagramMetricsService
{
    Task<InstagramMetricsDto?> GetMetricsAsync(
        string externalPostId,
        string externalUrl,
        CancellationToken ct = default);

    Task<InstagramPageMetricsDto?> GetPageTotalsAsync(
        InstagramPeriod period = InstagramPeriod.days_28,
        CancellationToken ct = default);
}