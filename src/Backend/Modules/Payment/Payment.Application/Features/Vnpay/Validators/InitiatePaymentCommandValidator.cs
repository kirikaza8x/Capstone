using FluentValidation;
using Payment.Domain.Enums;
using Payments.Application.Features.Commands.InitiatePayment;

namespace Payments.Application.Validators;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(500_000_000)
            .WithMessage("Amount cannot exceed 500,000,000 VND per transaction.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid payment type.");

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .WithMessage("IP address is required.");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("EventId is required for Direct Pay.")
            .When(x => x.Type == PaymentType.DirectPay);

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .WithMessage("Description cannot exceed 255 characters.")
            .When(x => x.Description != null);
    }
}
