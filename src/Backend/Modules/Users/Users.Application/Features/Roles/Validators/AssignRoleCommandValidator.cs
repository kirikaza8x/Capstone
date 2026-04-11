using FluentValidation;
using Users.Application.Features.Roles.Commands;

namespace Users.Application.Features.Roles.Validators
{
    public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("RoleId is required.");
        }
    }
}
