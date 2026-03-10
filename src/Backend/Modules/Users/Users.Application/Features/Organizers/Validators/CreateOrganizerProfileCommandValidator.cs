using FluentValidation;
using Users.Application.Features.Organizers.Commands;

namespace Users.Application.Features.Organizers.Validators;

public class CreateOrganizerProfileCommandValidator : AbstractValidator<CreateOrganizerProfileCommand>
{
    public CreateOrganizerProfileCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Organizer type must be a valid value.")
            // .NotEmpty()
            .WithMessage("Organizer type is required.");
    }
}
