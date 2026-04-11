using FluentValidation;
using Users.Application.Features.Users.Commands.Records;

namespace Users.Application.Features.Users.Validators;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("A valid email address is required.")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters.");
    }
}
