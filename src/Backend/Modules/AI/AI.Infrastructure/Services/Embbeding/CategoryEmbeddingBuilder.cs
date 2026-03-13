using AI.Application.Abstractions;
using AI.Domain.Entities;

namespace AI.Infrastructure.Embeddings;

/// <summary>
/// Builds category embeddings using the local ONNX embedding model.
/// Category embeddings are pre-computed for all categories and used as the basis for user preference modeling.
/// </summary>
public class CategoryEmbeddingBuilder
{
    private readonly IEmbeddingModel _embeddingModel;

    public CategoryEmbeddingBuilder(IEmbeddingModel embeddingModel)
    {
        _embeddingModel = embeddingModel;
    }

    public async Task<CategoryEmbedding> BuildAsync(
        string category,
        string description,
        CancellationToken cancellationToken = default)
    {
        var text = $"{category} {description}".Trim();

        // Generate embedding using the ONNX model
        var vector = await _embeddingModel.GenerateEmbeddingAsync(text, cancellationToken);

        return CategoryEmbedding.Create(
            category,
            description,
            vector);
    }

    /// <summary>
    /// Regenerates embedding using a new model version.
    /// Preserves existing stats (recommendation count, CTR) while updating the vector.
    /// </summary>
    public async Task<CategoryEmbedding> RegenerateAsync(
        CategoryEmbedding existing,
        string modelName = "all-MiniLM-L6-v2",
        CancellationToken cancellationToken = default)
    {
        var text = $"{existing.Category} {existing.Description}".Trim();
        var newVector = await _embeddingModel.GenerateEmbeddingAsync(text, cancellationToken);

        existing.UpdateEmbedding(newVector, modelName);
        return existing;
    }
}