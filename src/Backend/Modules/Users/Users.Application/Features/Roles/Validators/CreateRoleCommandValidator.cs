using FluentValidation;
using Users.Application.Features.Roles.Commands;

namespace Users.Application.Features.Roles.Validators
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Role name is required.")
                .MaximumLength(128)
                .WithMessage("Role name must not exceed 128 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(512)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 512 characters.");
        }
    }
}
