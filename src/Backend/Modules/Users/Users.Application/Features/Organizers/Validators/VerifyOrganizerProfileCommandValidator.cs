using FluentValidation;
using Users.Application.Features.Organizers.Commands;

namespace Users.Application.Features.Organizers.Validators;

public class VerifyOrganizerProfileCommandValidator
    : AbstractValidator<VerifyOrganizerProfileCommand>
{
    public VerifyOrganizerProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
