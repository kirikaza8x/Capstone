using FluentValidation;

namespace Users.Application.Features.Organizers.Commands.UpdateLogo;

public class UpdateLogoImageCommandValidator : AbstractValidator<UpdateLogoImageCommand>
{
    public UpdateLogoImageCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.");

        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .WithMessage("File name is required.");

        RuleFor(x => x.File.ContentType)
            .Must(BeValidImageType)
            .WithMessage("Only image files are allowed (jpg, jpeg, png, webp).");

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .WithMessage("File cannot be empty.")
            .LessThanOrEqualTo(5 * 1024 * 1024) // 5MB
            .WithMessage("File size must not exceed 5MB.");
    }

    private bool BeValidImageType(string contentType)
    {
        return contentType is "image/jpeg"
            or "image/png"
            or "image/jpg"
            or "image/webp";
    }
}