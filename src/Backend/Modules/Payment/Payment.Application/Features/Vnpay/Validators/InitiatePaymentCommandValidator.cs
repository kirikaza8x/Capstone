using FluentValidation;
using Payment.Domain.Enums;
using Payments.Application.Features.Payments.Commands.InitiatePayment;

namespace Payments.Application.Validators;

public class InitiatePaymentCommandValidator
    : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");

        RuleFor(x => x.Method)
            .Must(m => m == PaymentType.BatchDirectPay
                    || m == PaymentType.BatchWalletPay)
            .WithMessage("Method must be BatchDirectPay or BatchWalletPay.");

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .WithMessage("Description cannot exceed 255 characters.")
            .When(x => x.Description != null);
    }
}
