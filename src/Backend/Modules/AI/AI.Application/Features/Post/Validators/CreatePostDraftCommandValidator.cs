using FluentValidation;
using Marketing.Application.Posts.Commands;

public class CreatePostDraftCommandValidator 
    : AbstractValidator<CreatePostDraftCommand>
{
    public CreatePostDraftCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("EventId is required.");

        RuleFor(x => x.OrganizerId)
            .NotEmpty()
            .WithMessage("OrganizerId is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title is required and must not exceed 200 characters.");

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Body is required.");

        RuleFor(x => x.PromptUsed)
            .MaximumLength(2000)
            .When(x => x.PromptUsed != null);

        RuleFor(x => x.AiModel)
            .MaximumLength(100)
            .When(x => x.AiModel != null);

        RuleFor(x => x.AiTokensUsed)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AiTokensUsed.HasValue);
    }
}