using AI.Application.Abstractions;
using AI.Domain.Interfaces.UOW;
using Events.PublicApi.PublicApi;
using Marketing.Domain.Errors;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Commands;

public class GeneratePostDraftCommandHandler
    : ICommandHandler<GeneratePostDraftCommand, GeneratedPostDto>
{
    private readonly IEventTicketingPublicApi _eventApi;
    private readonly IGeminiService _geminiService;
    private readonly IAiTokenQuotaService _aiTokenQuotaService;
    private readonly IAiUnitOfWork _unitOfWork;
    private readonly ILogger<GeneratePostDraftCommandHandler> _logger;

    public GeneratePostDraftCommandHandler(
        IEventTicketingPublicApi eventApi,
        IGeminiService geminiService,
        IAiTokenQuotaService aiTokenQuotaService,
        IAiUnitOfWork unitOfWork,
        ILogger<GeneratePostDraftCommandHandler> logger)
    {
        _eventApi = eventApi;
        _geminiService = geminiService;
        _aiTokenQuotaService = aiTokenQuotaService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GeneratedPostDto>> Handle(
        GeneratePostDraftCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Fetch Event Details
        var ev = await _eventApi.GetEventDetailAsync(command.EventId, cancellationToken);
        if (ev is null)
        {
            return Result.Failure<GeneratedPostDto>(MarketingErrors.Post.EventIdRequired);
        }

        // 2. Prepare Data for Prompt
        var hashtags = string.Join(" ", ev.Hashtags ?? []);
        var categories = string.Join(", ", ev.Categories ?? []);

        var prompt = $"""
            Generate a marketing post draft in JSON format.
            Include: Title, Body, Summary, Slug, PromptUsed, AiModel, TrackingToken.
            Exclude: ImageUrl.
            User Requirement: {command.UserPromptRequirement ?? "No specific requirement"}
            Event: {ev.Title}
            Description: {ev.Description}
            Hashtags: {hashtags}
            Categories: {categories}
            """;

        try
        {
            // 3. Call AI Service (Now using the version that returns tokens)
            var geminiResult = await _geminiService.GenerateStructuredV2Async<GeneratedPostRequestDto>(
                prompt,
                cancellationToken: cancellationToken);

            var requestDto = geminiResult.Data;
            var actualTokens = geminiResult.TotalTokens;

            // 4. Credit Consumption
            if (actualTokens > 0)
            {
                var consumeResult = await _aiTokenQuotaService.ConsumeAsync(
                    command.OrganizerId,
                    actualTokens,
                    null,
                    cancellationToken);

                // Handle potential failure of the token consumption gracefully
                if (consumeResult.IsFailure)
                    return Result.Failure<GeneratedPostDto>(consumeResult.Error);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 5. Accurate Pricing (Gemini 1.5 Flash approx $0.00001875 per 1k tokens)
            // Using your specific multiplier:
            decimal estimatedCost = (actualTokens / 1000m) * 0.002m; 

            // 6. Map to Response DTO
            var result = new GeneratedPostDto
            {
                Title = requestDto.Title,
                Body = requestDto.Body,
                Summary = requestDto.Summary,
                PromptUsed = command.UserPromptRequirement,
                AiModel = _geminiService.GetModelInfo(),
                AiCost = estimatedCost,
                AiTokensUsed = actualTokens, 
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            // Log the error locally in the Marketing module
            _logger.LogError(ex, "Failed to generate post draft for Event {EventId}", command.EventId);

            // Return a Result.Failure instead of letting the middleware throw a 500
            return Result.Failure<GeneratedPostDto>(
                Error.Failure("AI.GenerationError", "The AI service failed to generate a post draft. Please try again."));
        }
    }
}
