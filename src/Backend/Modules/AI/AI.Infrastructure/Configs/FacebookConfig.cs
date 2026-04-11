// Marketing.Infrastructure/Configs/FacebookConfig.cs

using Shared.Infrastructure.Configs;

namespace Marketing.Infrastructure.Configs;

public sealed class FacebookConfig :ConfigBase
{
    public override string SectionName => "Facebook";
    public string PageAccessToken { get; init; } = string.Empty;
    public string PageId { get; init; } = string.Empty;
    public string GraphApiVersion { get; init; } = "v23.0";
    public string GraphApiBaseUrl { get; init; } = "https://graph.facebook.com";
}