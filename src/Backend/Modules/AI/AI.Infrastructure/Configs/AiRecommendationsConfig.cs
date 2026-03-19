using Shared.Infrastructure.Configs;

namespace AI.Infrastructure.Configs;

public class AiRecommendationsConfig : ConfigBase
{
    public override string SectionName => "RecommendationsConfig";
    public int MaxCandidates { get; set; } = 30;
    public int DefaultTopN { get; set; } = 10;
    public double DecayHalfLifeDays { get; set; } = 7.0;
}
