using FluentValidation;
using Users.Application.Features.Organizers.Commands;

namespace Users.Application.Features.Organizers.Validators;

public class CreateFullOrganizerProfileCommandValidator : AbstractValidator<CreateFullOrganizerProfileCommand>
{
    public CreateFullOrganizerProfileCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Organizer type must be a valid value.");

        When(x => x.BusinessInfo != null, () =>
        {
            RuleFor(x => x.BusinessInfo.DisplayName)
                .MaximumLength(200)
                .WithMessage("Display name is too long.");
            
            RuleFor(x => x.BusinessInfo.BusinessType)
                .IsInEnum()
                .When(x => x.BusinessInfo.BusinessType.HasValue)
                .WithMessage("Business type must be a valid value.");
        });

        When(x => x.BankInfo != null, () =>
        {
            RuleFor(x => x.BankInfo.AccountNumber)
                .MaximumLength(50)
                .WithMessage("Account number is too long.");
        });
    }
}