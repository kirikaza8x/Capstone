using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.ImageGeneration.Commands;

public sealed record GenerateImageCommand(
    string Prompt,
    string AspectRatio = "1:1",
    string ImageSize = "512x512"
) : ICommand<GenerateImageResponse>;

public sealed record GenerateImageResponse(string ImageUrl);