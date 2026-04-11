using FluentValidation;
using Users.Application.Features.Users.Commands.Records;

namespace Users.Application.Features.Users.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty()
                .WithMessage("Access token is required.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required.");

            RuleFor(x => x.DeviceId)
                .MaximumLength(128)
                .When(x => !string.IsNullOrWhiteSpace(x.DeviceId))
                .WithMessage("DeviceId must not exceed 128 characters.");

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
}
