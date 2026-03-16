namespace Shared.Application.Abstractions.Embbeding
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateAsync(string text, CancellationToken ct = default);
    }
}