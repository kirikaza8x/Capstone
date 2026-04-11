using FluentValidation;
using Payments.Application.Features.Refunds.Commands.MassRefundBySession;

namespace Payments.Application.Validators;

public class MassRefundByEventCommandValidator
    : AbstractValidator<MassRefundBySessionCommand>
{
    public MassRefundByEventCommandValidator()
    {
        RuleFor(x => x.EventSessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.AdminId)
            .NotEmpty().WithMessage("AdminId is required.");
    }
}
