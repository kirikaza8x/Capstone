using FluentValidation;
using Marketing.Application.Posts.Commands;

public class SubmitPostCommandValidator 
    : AbstractValidator<SubmitPostCommand>
{
    public SubmitPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.OrganizerId)
            .NotEmpty();
    }
}