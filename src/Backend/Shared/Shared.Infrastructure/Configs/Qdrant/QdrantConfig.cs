// AI.Infrastructure/Qdrant/QdrantOptions.cs
namespace Shared.Infrastructure.Configs.Qdrant;

public class QdrantConfig : ConfigBase
{
    public override string SectionName => "Qdrant";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public bool UseHttps { get; set; } = false;
    public string ApiKey { get; set; } = "";
    public int VectorSize { get; set; } = 384; // all-MiniLM-L6-v2 dimension
}