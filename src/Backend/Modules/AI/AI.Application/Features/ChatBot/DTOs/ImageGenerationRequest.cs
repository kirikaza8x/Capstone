namespace AI.Application.Features.ImageGeneration;

public sealed class ImageGenerationRequestDto
{
    public string Prompt      { get; init; } = default!;
    // public string AspectRatio { get; init; } = "1:1";
    // public string ImageSize   { get; init; } = "1K";
    
}

public sealed class ImageGenerationResult
{
    /// <summary>Base64 data URL: data:image/png;base64,...</summary>
    public string DataUrl { get; init; } = default!;

    /// <summary>Convenience: raw base64 without the prefix.</summary>
    public string Base64 => DataUrl.Contains(',')
        ? DataUrl[(DataUrl.IndexOf(',') + 1)..]
        : DataUrl;
}