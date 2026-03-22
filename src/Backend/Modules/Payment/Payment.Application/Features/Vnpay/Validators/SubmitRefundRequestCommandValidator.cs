using FluentValidation;
using Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;
using Payments.Domain.Enums;

namespace Payments.Application.Validators;

public class SubmitRefundRequestCommandValidator
    : AbstractValidator<SubmitRefundRequestCommand>
{
    public SubmitRefundRequestCommandValidator()
    {
        RuleFor(x => x.PaymentTransactionId)
            .NotEmpty().WithMessage("PaymentTransactionId is required.");

        RuleFor(x => x.Scope)
            .IsInEnum().WithMessage("Invalid refund scope.");

        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("EventId is required for single item refund.")
            .When(x => x.Scope == RefundRequestScope.SingleItem);

        RuleFor(x => x.UserReason)
            .NotEmpty().WithMessage("A reason is required.")
            .MinimumLength(10).WithMessage("Reason must be at least 10 characters.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
    }
}