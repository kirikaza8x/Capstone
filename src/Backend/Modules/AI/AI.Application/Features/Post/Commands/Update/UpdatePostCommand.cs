using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record UpdatePostCommand(
    Guid PostId,
    string? Title = null,
    string? Body = null,
    string? Summary = null,
    string? ImageUrl = null,
    string? Slug = null,
    string? PromptUsed = null,
    string? AiModel = null,
    int? AiTokensUsed = null,
    decimal? AiCost = null,
    string? TrackingToken = null
) : ICommand;


public record UpdatePostCommandV2(
    Guid PostId,
    bool GenerateWithAi, 
    string? UserPromptRequirement = null,
    string? Title = null,
    string? Body = null,
    string? Summary = null,
    string? Slug = null,
    string? AiModel = null,
    int? AiTokensUsed = null,
    decimal? AiCost = null,
    string? TrackingToken = null
) : ICommand;