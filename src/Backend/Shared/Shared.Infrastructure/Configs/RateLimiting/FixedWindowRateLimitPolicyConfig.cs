namespace Shared.Infrastructure.Configs;

public class FixedWindowRateLimitPolicyConfig
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int QueueLimit { get; set; }
}
