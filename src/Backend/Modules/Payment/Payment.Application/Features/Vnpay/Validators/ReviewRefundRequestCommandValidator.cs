using FluentValidation;
using Payments.Application.Features.Refunds.Commands.ReviewRefundRequest;

namespace Payments.Application.Validators;

public class ReviewRefundRequestCommandValidator
    : AbstractValidator<ReviewRefundRequestCommand>
{
    public ReviewRefundRequestCommandValidator()
    {
        RuleFor(x => x.RefundRequestId)
            .NotEmpty().WithMessage("RefundRequestId is required.");

        RuleFor(x => x.ReviewerNote)
            .NotEmpty().WithMessage("A reviewer note is required.")
            .MinimumLength(10).WithMessage("Note must be at least 10 characters.")
            .MaximumLength(1000).WithMessage("Note cannot exceed 1000 characters.");
    }
}
