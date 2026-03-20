using FluentValidation;
using Payments.Application.Features.Commands.RefundByEvent;

namespace Payments.Application.Validators;

public class RefundByEventCommandValidator : AbstractValidator<RefundByEventCommand>
{
    public RefundByEventCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("EventId is required.");
    }
}
