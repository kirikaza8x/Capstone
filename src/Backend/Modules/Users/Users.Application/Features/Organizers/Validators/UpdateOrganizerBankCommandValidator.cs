using FluentValidation;
using Users.Application.Features.Organizers.Commands;

namespace Users.Application.Features.Organizers.Validators;

public class UpdateOrganizerBankCommandValidator : AbstractValidator<UpdateOrganizerBankCommand>
{
    public UpdateOrganizerBankCommandValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .WithMessage("Account name is required.")
            .MaximumLength(256)
            .WithMessage("Account name must not exceed 256 characters.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account number is required.")
            .MaximumLength(64)
            .WithMessage("Account number must not exceed 64 characters.")
            .Matches(@"^\d+$")
            .WithMessage("Account number must contain only digits.");

        RuleFor(x => x.BankCode)
            .NotEmpty()
            .WithMessage("Bank code is required.")
            .MaximumLength(32)
            .WithMessage("Bank code must not exceed 32 characters.");

        RuleFor(x => x.Branch)
            .MaximumLength(128)
            .When(x => !string.IsNullOrWhiteSpace(x.Branch))
            .WithMessage("Branch must not exceed 128 characters.");
    }
}
