namespace Shared.Infrastructure.Configs;

public class RateLimitingConfig : ConfigBase
{
    public string GlobalPolicy { get; set; } = string.Empty;
    public Dictionary<string, FixedWindowRateLimitPolicyConfig> Policies { get; set; } = [];
}
