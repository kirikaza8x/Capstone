using FluentValidation;
using Marketing.Application.Posts.Commands;
using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Validators;

public class QueuePostForDistributionCommandValidator 
    : AbstractValidator<QueuePostForDistributionCommand>
{
    public QueuePostForDistributionCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.Platform)
            .NotEmpty()
            .WithMessage("Platform is required.")
            .NotEqual(ExternalPlatform.Unknown)
            .WithMessage("Platform cannot be Unknown.");
    }
}