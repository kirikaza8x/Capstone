using Marketing.Domain.Entities;
using Marketing.Domain.Enums;

namespace Marketing.Application.Services;

public interface IN8nDistributionService
{
    /// <summary>
    /// Sends distribution request to n8n webhook.
    /// Returns true if n8n accepted the request (2xx response).
    /// Service handles config, URL building, HTTP call internally.
    /// </summary>
    Task<bool> SendAsync(
        PostMarketing post,
        ExternalPlatform platform,
        CancellationToken ct = default);
}