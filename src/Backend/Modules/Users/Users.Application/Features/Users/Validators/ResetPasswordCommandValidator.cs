using FluentValidation;
using Users.Application.Features.Users.Commands.Records;

namespace Users.Application.Features.Users.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .Length(6)
            .WithMessage("OTP code must be exactly 6 digits.")
            .Matches(@"^\d{6}$")
            .WithMessage("OTP code must only contain numbers.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8) // Upping this to 8 is a good standard
                              // .WithMessage("New password must be at least 8 characters long.")
                              // .Matches(@"[A-Z]")
                              // .WithMessage("New password must contain at least one uppercase letter.")
                              // .Matches(@"[a-z]")
                              // .WithMessage("New password must contain at least one lowercase letter.")
                              // .Matches(@"[0-9]")
                              // .WithMessage("New password must contain at least one number.")
                              // .Matches(@"[\!\?\*\.]")
                              // .WithMessage("New password must contain at least one special character (!?*.).")
            ;
    }
}
