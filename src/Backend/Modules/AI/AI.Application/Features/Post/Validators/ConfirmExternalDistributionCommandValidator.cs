using FluentValidation;
using Marketing.Application.Posts.Commands;

namespace Marketing.Application.Posts.Validators;

public class ConfirmExternalDistributionCommandValidator 
    : AbstractValidator<ConfirmExternalDistributionCommand>
{
    public ConfirmExternalDistributionCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Platform).NotEmpty().NotEqual(Marketing.Domain.Enums.ExternalPlatform.Unknown);
        RuleFor(x => x.ExternalUrl).NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.ExternalUrl));
    }
}