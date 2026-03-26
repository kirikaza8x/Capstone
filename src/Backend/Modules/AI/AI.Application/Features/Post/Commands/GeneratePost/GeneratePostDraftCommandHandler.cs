using AI.Application.Abstractions;
using Events.PublicApi.PublicApi;
using Marketing.Domain.Errors;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Commands;


public class GeneratePostDraftCommandHandler
    : ICommandHandler<GeneratePostDraftCommand, GeneratedPostDto>
{
    private readonly IEventTicketingPublicApi _eventApi;
    private readonly IGeminiService _geminiService;

    public GeneratePostDraftCommandHandler(
        IEventTicketingPublicApi eventApi,
        IGeminiService geminiService)
    {
        _eventApi = eventApi;
        _geminiService = geminiService;
    }

    public async Task<Result<GeneratedPostDto>> Handle(
        GeneratePostDraftCommand command,
        CancellationToken cancellationToken)
    {
        // Call the Events module via its public API contract
        var ev = await _eventApi.GetEventDetailAsync(command.EventId, cancellationToken);
        if (ev is null)
            return Result.Failure<GeneratedPostDto>(
                MarketingErrors.Post.EventIdRequired);

        var hashtags = string.Join(" ", ev.Hashtags);
        var categories = string.Join(", ", ev.Categories);

        var prompt = $"""
        Generate a marketing post draft in JSON format.
        Include Title, Body, Summary, Slug, PromptUsed, AiModel, TrackingToken.
        Exclude ImageUrl.
        Event: {ev.Title}
        Description: {ev.Description}
        Hashtags: {hashtags}
        Categories: {categories}
        """;

        var result = await _geminiService.GenerateStructuredAsync<GeneratedPostDto>(
            prompt,
            cancellationToken: cancellationToken);

        // Approximate token usage and cost
        int estimatedTokens = (prompt.Length + (result.Body?.Length ?? 0)) / 4;
        decimal estimatedCost = (estimatedTokens / 1000m) * 0.002m; // Example pricing

        result.AiTokensUsed = estimatedTokens;
        result.AiCost = estimatedCost;

        return Result.Success(result);
    }
}
