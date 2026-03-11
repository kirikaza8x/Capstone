using FluentValidation;

namespace Users.Application.Features.Organizers.Validators;

public class BeginOrganizerProfileUpdateCommandValidator
    : AbstractValidator<BeginOrganizerProfileUpdateCommand>
{
    public BeginOrganizerProfileUpdateCommandValidator()
    {
        // No input fields.
        // Domain rules are enforced inside the aggregate.
    }
}