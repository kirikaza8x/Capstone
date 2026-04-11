using FluentValidation;
using Payments.Application.Features.Payments.Commands.GetPaymentUrl;

namespace Payments.Application.Validators;

public class GetPaymentUrlCommandValidator
    : AbstractValidator<GetPaymentUrlCommand>
{
    public GetPaymentUrlCommandValidator()
    {
        RuleFor(x => x.PaymentTransactionId)
            .NotEmpty()
            .WithMessage("PaymentTransactionId is required.");
    }
}
