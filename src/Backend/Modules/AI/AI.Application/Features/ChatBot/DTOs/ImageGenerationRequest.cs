using System.ComponentModel;

namespace AI.Application.Features.ImageGeneration;

public sealed class ImageGenerationRequestDto
{
    public string Prompt { get; set; } = default!;
    [DefaultValue("1:1")]
    public string AspectRatio { get; set; } = "1:1";
    [DefaultValue("512x512")]
    public string ImageSize { get; set; } = "512x512";
}

public sealed class ImageGenerationResult
{
    public string? ImageUrl { get; set; }
}
