using Shared.Infrastructure.Configs;

public class JwtConfigs : ConfigBase
{
    public override string SectionName => "JwtConfigs";
    public string Secret { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpiryMinutes { get; init; }
    public int RefreshTokenExpiryDays { get; init; } = 7;
}
