namespace Shared.Infrastructure.Configs.Security
{
    public class GoogleAuthConfigs : ConfigBase
    {
        public override string SectionName => "GoogleAuthConfigs";
        public string ServerClientId { get; init; } = default!;
        public string? AndroidClientId { get; init; }   // optional
    }
}
