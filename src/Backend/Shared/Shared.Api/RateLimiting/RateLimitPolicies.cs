namespace Shared.Api.RateLimiting;

public static class RateLimitPolicies
{
    public const string Global = "Global";
    public const string Auth = "Auth";
    public const string AiGenerate = "AiGenerate";
    public const string Payment = "Payment";
    public const string Webhook = "Webhook";
    public const string Order = "Order";
}
