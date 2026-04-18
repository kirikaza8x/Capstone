namespace Shared.Infrastructure.Configs.Security
{
    public class GoogleAuthConfigs : ConfigBase
    {
        public override string SectionName => "Authentication:Google";
        public string ServerClientId { get; init; } = default!;
        public string? AndroidClientId { get; init; }   // optional
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}
