using FluentValidation;
using Payments.Application.Features.Commands.WalletPay;

namespace Payments.Application.Validators;

public class WalletPayCommandValidator : AbstractValidator<WalletPayCommand>
{
    public WalletPayCommandValidator()
    {
        // RuleFor(x => x.UserId)
        //     .NotEmpty()
        //     .WithMessage("UserId is required.");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("EventId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(500_000_000)
            .WithMessage("Amount cannot exceed 500,000,000 VND per transaction.");

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .WithMessage("Description cannot exceed 255 characters.")
            .When(x => x.Description != null);
    }
}
