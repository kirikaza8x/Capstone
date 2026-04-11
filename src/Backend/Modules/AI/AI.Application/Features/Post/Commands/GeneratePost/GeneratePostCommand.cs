
using Shared.Application.Abstractions.Messaging;

public record GeneratePostDraftCommand(
    Guid EventId,
    Guid OrganizerId,
    string? UserPromptRequirement    
    // string? SystemPromptOverride = null
) : ICommand<GeneratedPostDto>;

