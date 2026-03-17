namespace Shared.Application.Abstractions.Embbeding
{
    public interface IEmbeddingService
    {
        /// <summary>Embed a single text string.</summary>
        Task<float[]> EmbedAsync(string text, CancellationToken ct = default);

        /// <summary>
        /// Embed multiple texts in one batched call.
        /// Prefer this over looping EmbedAsync — most providers have batch endpoints.
        /// </summary>
        Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
    }
}