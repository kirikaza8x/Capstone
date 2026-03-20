using FluentValidation;
using Payments.Application.Features.Commands.MassRefundByEvent;

namespace Payments.Application.Validators;

public class MassRefundByEventCommandValidator : AbstractValidator<MassRefundByEventCommand>
{
    public MassRefundByEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("EventId is required.");
    }
}
