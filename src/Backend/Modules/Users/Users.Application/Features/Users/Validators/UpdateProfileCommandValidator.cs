using Users.Application.Features.Users.Commands.Records;
using FluentValidation;

namespace Users.Application.Features.Users.Validators;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName))
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName))
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{7,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Address)
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Address))
            .WithMessage("Address must not exceed 256 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description must not exceed 512 characters.");

        RuleFor(x => x.SocialLink)
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.SocialLink))
            .WithMessage("Social link must not exceed 256 characters.");

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.ProfileImageUrl))
            .WithMessage("Profile image URL must not exceed 512 characters.");
    }
}
