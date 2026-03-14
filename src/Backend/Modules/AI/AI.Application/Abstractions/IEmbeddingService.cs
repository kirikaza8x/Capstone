// AI.Application/Abstractions/IEmbeddingService.cs
namespace AI.Application.Abstractions
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateAsync(string text, CancellationToken ct = default);
    }
}