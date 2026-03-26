using FluentValidation;
using Marketing.Application.Posts.Commands;

public class UpdatePostCommandValidator 
    : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.OrganizerId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty();
        RuleFor(x => x.Summary)
            .MaximumLength(500);
    }
}