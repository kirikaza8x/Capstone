using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record CreatePostDraftCommand(
    Guid EventId,
    Guid OrganizerId,
    string Title,
    string Body,    
    string? Summary = null,
    string? PromptUsed = null,
    string? AiModel = null,
    int? AiTokensUsed = null
) : ICommand<Guid>;