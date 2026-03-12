using Shared.Infrastructure.Configs;

namespace AI.Infrastructure.Configs;

public class OpenRouterConfig : ConfigBase
{
    public override string SectionName => "OpenRouter";
    public string ApiKey { get; set; } = default!;
    public string Model { get; set; } = default!;
}