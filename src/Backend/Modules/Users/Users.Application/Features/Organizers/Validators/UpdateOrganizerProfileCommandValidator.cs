using FluentValidation;
using Users.Application.Features.Organizers.Commands;

namespace Users.Application.Features.Organizers.Validators;

public class UpdateOrganizerProfileCommandValidator : AbstractValidator<UpdateOrganizerProfileCommand>
{
    public UpdateOrganizerProfileCommandValidator()
    {
        RuleFor(x => x.Logo)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.Logo))
            .WithMessage("Logo URL must not exceed 512 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(256)
            .WithMessage("Display name must not exceed 256 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1024)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description must not exceed 1024 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.Address))
            .WithMessage("Address must not exceed 512 characters.");

        RuleFor(x => x.SocialLink)
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.SocialLink))
            .WithMessage("Social link must not exceed 256 characters.");

        // RuleFor(x => x.BusinessType.)
        //     .MaximumLength(128)
        //     .When(x => !string.IsNullOrWhiteSpace(x.BusinessType))
        //     .WithMessage("Business type must not exceed 128 characters.");

        RuleFor(x => x.TaxCode)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxCode))
            .WithMessage("Tax code must not exceed 64 characters.");

        RuleFor(x => x.IdentityNumber)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
            .WithMessage("Identity number must not exceed 64 characters.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyName))
            .WithMessage("Company name must not exceed 256 characters.");
    }
}
