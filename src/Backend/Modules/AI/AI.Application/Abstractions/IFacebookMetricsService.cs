// Marketing.Application/Services/IFacebookMetricsService.cs

using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.Services;

public interface IFacebookMetricsService
{
    Task<FacebookMetricsDto?> GetMetricsAsync(
        string externalPostId,
        string externalUrl,
        CancellationToken ct = default);
}