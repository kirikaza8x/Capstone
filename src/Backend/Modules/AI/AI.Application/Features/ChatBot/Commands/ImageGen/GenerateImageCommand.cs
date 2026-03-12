using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.ImageGeneration.Commands;

public sealed record GenerateImageCommand(
    string Prompt,
    string? AspectRatio = "1:1",
    string? ImageSize = "1K"
) : ICommand<IReadOnlyList<GenerateImageResponse>>;

public sealed record GenerateImageResponse(string DataUrl, string Base64);