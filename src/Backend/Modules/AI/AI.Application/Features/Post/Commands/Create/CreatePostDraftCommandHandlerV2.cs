using AI.Application.Abstractions;
using AI.Domain.Interfaces.UOW;
using Events.PublicApi.PublicApi;
using Marketing.Application.Posts.Commands;
using Marketing.Domain.Entities;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Helpers;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Handlers;

public class CreatePostDraftCommandHandlerV2 : ICommandHandler<CreatePostDraftCommandV2, Guid>
{
    private readonly IPostRepository _postRepository;
    private readonly IEventTicketingPublicApi _eventApi;
    private readonly IGeminiService _geminiService;
    private readonly IAiUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePostDraftCommandHandlerV2> _logger;

    public CreatePostDraftCommandHandlerV2(
        IPostRepository postRepository,
        IEventTicketingPublicApi eventApi,
        IGeminiService geminiService,
        IAiUnitOfWork unitOfWork,
        ILogger<CreatePostDraftCommandHandlerV2> logger)
    {
        _postRepository = postRepository;
        _eventApi = eventApi;
        _geminiService = geminiService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreatePostDraftCommandV2 command, CancellationToken ct)
    {
        // 1. Fetch Event Context with full metadata
        var ev = await _eventApi.GetEventDetailAsync(command.EventId, ct);
        if (ev is null)
            return Result.Failure<Guid>(MarketingErrors.Post.EventIdRequired);

        var hashtags = string.Join(" ", ev.Hashtags ?? []);
        var categories = string.Join(", ", ev.Categories ?? []);

        // 2. Initialize defaults
        string finalTitle = command.Title ?? "";
        string finalBody = command.Body ?? "";
        string? aiModel = null;
        int? aiTokensUsed = null;
        decimal? aiCost = null;

        // 3. AI Generation Logic
        if (command.GenerateWithAi)
        {
            try
            {
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

                var aiResult = await _geminiService.GenerateStructuredV2Async<GeneratedPostRequestDto>(prompt, null, ct);
                
                finalTitle = aiResult.Data.Title ?? "";
                finalBody = aiResult.Data.Body ?? "";
                aiTokensUsed = aiResult.TotalTokens;
                aiModel = _geminiService.GetModelInfo();
                aiCost = (aiTokensUsed / 1000m) * 0.002m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Creation failed for Event {EventId}", command.EventId);
                return Result.Failure<Guid>(Error.Failure("AI.CreateError", "The AI service failed. Please check your prompt."));
            }
        }

        // 4. Validation
        if (string.IsNullOrWhiteSpace(finalTitle))
            return Result.Failure<Guid>(MarketingErrors.Post.TitleCannotBeEmpty);

        // 5. Slug & Token Generation
        var baseSlug = SlugHelper.Generate(finalTitle);
        var uniqueSlug = await GenerateUniqueSlug(baseSlug, ct);
        var trackingToken = command.TrackingToken ?? Guid.NewGuid().ToString("N").Substring(0, 8);

        // 6. Persistence
        var post = PostMarketing.CreateDraft(
            eventId: command.EventId,
            organizerId: command.OrganizerId,
            title: finalTitle,
            body: finalBody,
            trackingToken: trackingToken,
            summary: command.Summary,
            slug: uniqueSlug,
            promptUsed: command.UserPromptRequirement,
            aiModel: aiModel,
            aiTokensUsed: aiTokensUsed,
            aiCost: aiCost
        );

        _postRepository.Add(post);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(post.Id);
    }

    private async Task<string> GenerateUniqueSlug(string baseSlug, CancellationToken ct)
    {
        var slug = baseSlug;
        int counter = 1;
        while (await _postRepository.SlugExistsAsync(slug, ct))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }
}