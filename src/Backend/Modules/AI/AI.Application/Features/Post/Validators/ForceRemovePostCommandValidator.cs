using FluentValidation;
using Marketing.Application.Posts.Commands;

public class ForceRemovePostCommandValidator 
    : AbstractValidator<ForceRemovePostCommand>
{
    public ForceRemovePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.AdminId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}