
using Shared.Infrastructure.Configs;

namespace Marketing.Infrastructure.Configs;

public class N8nIntegrationConfig : ConfigBase
{
    public override string SectionName => "N8nIntegration";
    
    /// <summary>
    /// Full webhook URL for post distribution (e.g., https://n8n.yourdomain.com/webhook/post-distribute)
    /// </summary>
    public string DistributionWebhookUrl { get; init; } = default!;
    
    /// <summary>
    /// API key for authenticating requests to n8n (optional, if webhook is protected)
    /// </summary>
    public string? ApiKey { get; init; }
    
    /// <summary>
    /// Your app's public base URL (for building public post links sent to n8n)
    /// </summary>
    public string AppBaseUrl { get; init; } = default!;
    
    /// <summary>
    /// Timeout in seconds for HTTP calls to n8n (default: 30)
    /// </summary>
    public int HttpTimeoutSeconds { get; init; } = 30;
}