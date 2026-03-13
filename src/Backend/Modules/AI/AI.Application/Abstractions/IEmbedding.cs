using AI.Domain.Entities;

namespace AI.Application.Abstractions;

/// <summary>
/// Represents an external embedding model (OpenAI, HuggingFace, local model).
/// Currently unused — kept for future integration.
/// </summary>
public interface IEmbeddingModel
{
    Task<float[]> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}

public interface ILocalVectorBuilder
{
    float[] BuildVector(string key);
}

public interface IUserEmbeddingBuilder
{
    float[] Build(List<UserInterestScore> scores);
}