using FluentValidation;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Validators;

public class FailExternalDistributionCommandValidator 
    : AbstractValidator<FailExternalDistributionCommand>
{
    public FailExternalDistributionCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Platform).NotEmpty().NotEqual(Marketing.Domain.Enums.ExternalPlatform.Unknown);
        RuleFor(x => x.ErrorMessage).NotEmpty().MaximumLength(500);
    }
}