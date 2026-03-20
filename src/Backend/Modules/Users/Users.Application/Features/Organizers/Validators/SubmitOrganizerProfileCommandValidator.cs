using FluentValidation;

namespace Users.Application.Features.Organizers.Validators;

public class SubmitOrganizerProfileCommandValidator
    : AbstractValidator<SubmitOrganizerProfileCommand>
{
    public SubmitOrganizerProfileCommandValidator()
    {
        // No fields to validate yet.
        // This validator exists for pipeline consistency.
    }
}
