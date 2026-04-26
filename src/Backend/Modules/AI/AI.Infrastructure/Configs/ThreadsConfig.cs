using Shared.Infrastructure.Configs;

namespace Marketing.Infrastructure.Configs;

public sealed class ThreadsConfig : ConfigBase
{
    public override string SectionName => "Threads";

    public string AccessToken { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string GraphApiVersion { get; init; } = "v1.0";
    public string GraphApiBaseUrl { get; init; } = "https://graph.threads.net";
}