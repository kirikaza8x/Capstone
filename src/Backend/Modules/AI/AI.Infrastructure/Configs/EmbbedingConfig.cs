using Shared.Infrastructure.Configs;

namespace AI.Infrastructure.Configs;

public class EmbeddingConfig : ConfigBase
{
    public override string SectionName => "Embedding:Queue";
    public string RequestQueue { get; set; } = "embedding.requests";
    public string ResponseQueue { get; set; } = "embedding.responses";
    public int TimeoutSeconds { get; set; } = 30;
}
