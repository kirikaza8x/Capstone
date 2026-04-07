using AI.Application.Abstractions;
using AI.Domain.Interfaces.UOW;
using Events.PublicApi.PublicApi;
using Marketing.Application.Posts.Commands;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Helpers;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Handlers;

public class UpdatePostCommandV2Handler : ICommandHandler<UpdatePostCommandV2>
{
    private readonly IPostRepository _postRepository;
    private readonly IEventTicketingPublicApi _eventApi;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAiTokenQuotaService _aiTokenQuotaService;
    private readonly IAiUnitOfWork _unitOfWork;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<UpdatePostCommandV2Handler> _logger;

    public UpdatePostCommandV2Handler(
        IPostRepository postRepository,
        IEventTicketingPublicApi eventApi,
        ICurrentUserService currentUserService,
        IAiTokenQuotaService aiTokenQuotaService,
        IAiUnitOfWork unitOfWork,
        IGeminiService geminiService,
        ILogger<UpdatePostCommandV2Handler> logger)
    {
        _postRepository = postRepository;
        _eventApi = eventApi;
        _currentUserService = currentUserService;
        _aiTokenQuotaService = aiTokenQuotaService;
        _unitOfWork = unitOfWork;
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdatePostCommandV2 command, CancellationToken ct)
    {
        var user = _currentUserService.GetCurrentUser();
        var post = await _postRepository.GetByIdAsync(command.PostId, ct);

        if (post is null) 
            return Result.Failure(MarketingErrors.Post.NotFound(command.PostId));

        // 1. Fetch Event Context (just like Create)
        var ev = await _eventApi.GetEventDetailAsync(post.EventId, ct);
        if (ev is null)
            return Result.Failure(MarketingErrors.Post.EventIdRequired);

        var hashtags = string.Join(" ", ev.Hashtags ?? []);
        var categories = string.Join(", ", ev.Categories ?? []);

        // 2. Initialize with existing or command values
        string finalTitle = command.Title ?? post.Title;
        string finalBody = command.Body ?? post.Body;
        int? additionalTokens = null;
        decimal? additionalCost = null;

        // 3. AI Update Logic with Rich Context
        if (command.GenerateWithAi)
        {
            try
            {
                var prompt = $"""
                    Update the following marketing post draft in JSON format based on the instruction.
                    Include: Title, Body, Summary, Slug.
                    
                    [Update Instruction]: {command.UserPromptRequirement ?? "Improve the content quality and engagement."}
                    
                    [Current Title]: {post.Title}
                    [Current Content]: {post.Body}
                    
                    [Event Context]:
                    Event Name: {ev.Title}
                    Description: {ev.Description}
                    Hashtags: {hashtags}
                    Categories: {categories}
                    """;

                var aiResult = await _geminiService.GenerateStructuredV2Async<GeneratedPostRequestDto>(prompt, null, ct);
                
                // Fix CS8600: Fallback for AI null returns
                finalTitle = aiResult.Data.Title ?? ""; 
                finalBody = aiResult.Data.Body ?? "";
                
                additionalTokens = aiResult.TotalTokens;
                additionalCost = (additionalTokens / 1000m) * 0.002m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Update failed for post {PostId}", post.Id);
                return Result.Failure(Error.Failure("AI.UpdateError", "The AI service failed to update content."));
            }
        }

        // 4. Slug & Validation Logic
        string? finalSlug = post.Slug;
        if (!string.Equals(post.Title, finalTitle, StringComparison.Ordinal))
        {
            var baseSlug = SlugHelper.Generate(finalTitle);
            finalSlug = await GenerateUniqueSlug(baseSlug, ct);
        }

        // 5. Domain Update (Accumulates AI metrics)
        var result = post.Update(
            title: finalTitle,
            body: finalBody,
            summary: command.Summary,
            imageUrl: null,
            slug: finalSlug,
            promptUsed: command.UserPromptRequirement ?? post.PromptUsed,
            aiModel: _geminiService.GetModelInfo(),
            additionalTokensUsed: additionalTokens,
            additionalAiCost: additionalCost,
            trackingToken: command.TrackingToken ?? post.TrackingToken
        );

        if (result.IsFailure)
            return result;

        // Consume AI tokens if generated new content
        if (command.GenerateWithAi && additionalTokens.GetValueOrDefault() > 0)
        {
            var consumeResult = await _aiTokenQuotaService.ConsumeAsync(
                post.OrganizerId,
                additionalTokens.Value,
                post.Id,
                ct);

            if (consumeResult.IsFailure)
                return Result.Failure(consumeResult.Error);
        }

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
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
