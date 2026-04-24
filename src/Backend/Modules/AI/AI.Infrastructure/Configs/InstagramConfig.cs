using Shared.Infrastructure.Configs;

namespace Marketing.Infrastructure.Configs;

public class InstagramConfig : ConfigBase
{
    public override string SectionName => "Instagram";

    public string PageAccessToken { get; init; } = string.Empty;
    public string AccountId { get; init; } = string.Empty; // Instagram Business Account ID
    public string PageId { get; init; } = string.Empty;    // Linked Facebook Page ID
    public string GraphApiVersion { get; init; } = "v23.0";
    public string GraphApiBaseUrl { get; init; } = "https://graph.facebook.com";
}