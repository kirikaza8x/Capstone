using FluentValidation;

namespace AI.Application.Features.AiPackages.Commands.UpdateAiPackage;

public sealed class UpdateAiPackageCommandValidator : AbstractValidator<UpdateAiPackageCommand>
{
    public UpdateAiPackageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TokenQuota).GreaterThan(0);
    }
}
