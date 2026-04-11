using FluentValidation;
using Marketing.Application.Posts.Commands;

public class PublishPostCommandValidator 
    : AbstractValidator<PublishPostCommand>
{
    public PublishPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.OrganizerId)
            .NotEmpty();
    }
}