// Marketing.Application/Services/IFacebookMetricsService.cs

using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;

namespace Marketing.Application.Services;

public interface IFacebookMetricsService
{
    Task<FacebookMetricsDto?> GetMetricsAsync(
        string externalPostId,
        string externalUrl,
        CancellationToken ct = default);

    Task<string?> GetPageAccessTokenAsync(HttpClient client, CancellationToken ct);

    Task<FacebookPageMetricsDto?> GetPageTotalsAsync(FacebookPeriod period,CancellationToken ct = default);
}