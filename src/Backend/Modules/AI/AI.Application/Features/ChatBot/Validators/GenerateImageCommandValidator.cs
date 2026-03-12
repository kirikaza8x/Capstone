using AI.Application.Features.ImageGeneration.Commands;
using FluentValidation;

namespace AI.Application.Features.ChatBot;

public sealed class GenerateImageCommandValidator : AbstractValidator<GenerateImageCommand>
{

    // private static readonly HashSet<string> ValidImageSizes =
    // new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    // {
    //     "0.5K", "1K", "2K", "4K"
    // };

    // private static readonly HashSet<string> ValidAspectRatios =
    //     new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    //     {
    //     "1:1", "2:3", "3:2", "3:4", "4:3",
    //     "4:5", "5:4", "9:16", "16:9", "21:9"
    //     };


    public GenerateImageCommandValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(2000).WithMessage("Prompt must not exceed 2000 characters.");

        // RuleFor(x => x.AspectRatio)
        //     .NotEmpty()
        //     .Must(v => ValidAspectRatios.Contains(v))
        //     .WithMessage($"AspectRatio must be one of: {string.Join(", ", ValidAspectRatios)}");

        // RuleFor(x => x.ImageSize)
        //     .NotEmpty()
        //     .Must(v => ValidImageSizes.Contains(v))
        //     .WithMessage($"ImageSize must be one of: {string.Join(", ", ValidImageSizes)}");
    }
}