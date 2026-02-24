using Users.Application.Features.Users.Commands.Records;
using FluentValidation;

namespace Users.Application.Features.Users.Validators;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty()
            .WithMessage("Email or username is required.")
            .MaximumLength(256)
            .WithMessage("Email or username must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.DeviceName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceName))
            .WithMessage("DeviceName must not exceed 128 characters.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress))
            .WithMessage("IpAddress must not exceed 64 characters.");

        RuleFor(x => x.UserAgent)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.UserAgent))
            .WithMessage("UserAgent must not exceed 512 characters.");
    }
}



