using AI.Application.Features.ImageGeneration;

namespace AI.Application.Abstractions;

public interface IImageGenerationService
{
    /// <summary>
    /// Returns base64 data-URL strings (data:image/png;base64,...).
    /// </summary>
    Task<IReadOnlyList<ImageGenerationResult>> GenerateImagesAsync(
        ImageGenerationRequestDto request,
        CancellationToken cancellationToken = default
    );
}
