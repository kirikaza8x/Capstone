using FluentValidation;
using Users.Application.Features.Users.Commands.Records;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    private static readonly string[] AllowedContentTypes =
        { "image/jpeg", "image/png", "image/gif", "image/webp" };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(f => f.Length > 0).WithMessage("File is required.")
            .Must(f => f.Length <= MaxFileSize).WithMessage("File too large. Max size is 10 MB.")
            .Must(f => AllowedContentTypes.Contains(f.ContentType.ToLowerInvariant()))
            .WithMessage("Invalid file type. Allowed: jpeg, png, gif, webp.");
    }
}
