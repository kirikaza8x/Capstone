using FluentValidation;

namespace AI.Application.Features.AiPackages.Commands.ToggleAiPackageStatus;

public sealed class ToggleAiPackageStatusCommandValidator : AbstractValidator<ToggleAiPackageStatusCommand>
{
    public ToggleAiPackageStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
