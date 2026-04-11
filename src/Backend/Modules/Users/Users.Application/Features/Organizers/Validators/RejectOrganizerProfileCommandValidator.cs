using FluentValidation;

namespace Users.Application.Features.Organizers.Validators;

public class RejectOrganizerProfileCommandValidator
    : AbstractValidator<RejectOrganizerProfileCommand>
{
    public RejectOrganizerProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required.")
            .MaximumLength(500)
            .WithMessage("Rejection reason must not exceed 500 characters.");
    }
}
