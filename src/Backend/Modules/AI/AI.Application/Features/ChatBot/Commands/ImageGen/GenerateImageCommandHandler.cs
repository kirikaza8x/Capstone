using AI.Application.Abstractions;
using AI.Application.Features.ImageGeneration.Commands;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.ImageGeneration.Handlers;

public sealed class GenerateImageCommandHandler
    : ICommandHandler<GenerateImageCommand, IReadOnlyList<GenerateImageResponse>>
{
    private readonly IImageGenerationService _imageService;

    public GenerateImageCommandHandler(
        IImageGenerationService imageService,
        IValidator<GenerateImageCommand> validator)
    {
        _imageService = imageService;
    }

    public async Task<Result<GenerateImageResponse>> Handle(
        GenerateImageCommand command,
        CancellationToken cancellationToken)
    {
        var request = new ImageGenerationRequestDto
        {
            Prompt      = command.Prompt,
            AspectRatio = command.AspectRatio,
            ImageSize   = command.ImageSize
        };

        var results = await _imageService.GenerateImagesAsync(request, cancellationToken);

        if (results.Count == 0)
        {
            return Result.Failure<GenerateImageResponse>(
                Error.Failure("ImageGeneration.Empty", "No images were returned from the provider."));
        }

        var response = results
            .Select(r => new GenerateImageResponse(r.ImageUrl ?? string.Empty))
            .ToList();

        return Result.Success<IReadOnlyList<GenerateImageResponse>>(response);
    }
}