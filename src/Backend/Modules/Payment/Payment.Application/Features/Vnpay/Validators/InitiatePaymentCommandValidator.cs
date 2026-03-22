using FluentValidation;
using Payment.Domain.Enums;
using Payments.Application.Features.Payments.Commands.InitiatePayment;

namespace Payments.Application.Validators;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {

        // RuleFor(x => x.IpAddress)
        //     .NotEmpty().WithMessage("IP address is required.");

        // RuleFor(x => x.Method)
        //     .Must(m => m == PaymentType.BatchDirectPay || m == PaymentType.BatchWalletPay)
        //     .WithMessage("Method must be BatchDirectPay or BatchWalletPay.");

        // RuleFor(x => x.Items)
        //     .NotEmpty().WithMessage("At least one item is required.");

        // RuleForEach(x => x.Items).ChildRules(item =>
        // {
        //     item.RuleFor(i => i.EventId)
        //         .NotEmpty().WithMessage("EventId is required.");

        //     item.RuleFor(i => i.Amount)
        //         .GreaterThan(0).WithMessage("Amount must be greater than zero.")
        //         .LessThanOrEqualTo(500_000_000)
        //         .WithMessage("Item amount cannot exceed 500,000,000 VND.");
        // });

        // RuleFor(x => x.Items)
        //     .Must(items => items.Select(i => i.EventId).Distinct().Count() == items.Count)
        //     .WithMessage("Duplicate EventIds are not allowed in the same payment.")
        //     .When(x => x.Items?.Count > 0);

        // RuleFor(x => x.Description)
        //     .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.")
        //     .When(x => x.Description != null);
    }
}