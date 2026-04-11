using FluentValidation;
using Marketing.Application.Posts.Commands;

public class ApprovePostCommandValidator 
    : AbstractValidator<ApprovePostCommand>
{
    public ApprovePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.AdminId)
            .NotEmpty();
    }
}