using FluentValidation;
using Users.Application.Features.Users.Commands.Records;

namespace Users.Application.Features.Users.Validators;
public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty()
                .WithMessage("Google ID token is required.")
                .MinimumLength(100)
                .WithMessage("Invalid Google ID token format.")
                .Must(token => !token.Contains(" "))
                .WithMessage("Google ID token cannot contain spaces.");
        }
    }