using FluentValidation;

namespace AI.Application.Features.AiPackages.Commands.CreateAiPackage;

public sealed class CreateAiPackageCommandValidator : AbstractValidator<CreateAiPackageCommand>
{
    public CreateAiPackageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TokenQuota).GreaterThan(0);
    }
}
