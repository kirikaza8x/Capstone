namespace AI.Application.Models
{
    /// <summary>
    /// Holds dynamic weights for algorithm blending
    /// </summary>
    public class AlgorithmWeights
    {
        public double RuleWeight { get; set; }
        public double MLWeight { get; set; }
        public double EmbeddingWeight { get; set; }

        public bool UseML { get; set; }
        public bool UseEmbeddings { get; set; }

        public string Phase { get; set; } = "Unknown";
        public string Reason { get; set; } = "";
    }
}