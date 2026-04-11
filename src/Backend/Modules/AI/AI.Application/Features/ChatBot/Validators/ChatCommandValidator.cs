using FluentValidation;

namespace AI.Application.Features.ChatBot;

public sealed class ChatCommandValidator : AbstractValidator<ChatCommand>
{
    public ChatCommandValidator()
    {
        RuleFor(x => x.UserPrompt)
            .NotEmpty().WithMessage("UserPrompt is required.")
            .MinimumLength(2).WithMessage("UserPrompt must be at least 2 characters long.")
            .MaximumLength(2000).WithMessage("UserPrompt must not exceed 2000 characters.");
    }
}
