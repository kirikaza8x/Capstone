using AI.Domain.Events;
using AI.Domain.Repositories;
using AI.Domain.Helpers;  
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using AI.Domain.Interfaces.UOW;
using System.Collections.ObjectModel;

namespace AI.Application.Features.Tracking.EventHandlers;

/// <summary>
/// Handles BehaviorLogCreatedEvent to update user interest scores.
/// Runs synchronously within the domain transaction boundary.
/// 
/// RESPONSIBILITIES:
/// - Extract categories from metadata using MetadataHelper
/// - Get action weight from InteractionWeight config
/// - Upsert UserInterestScore with decay + add (atomic)
/// - Mark UserEmbedding as stale (triggers rebuild job later)
/// 
/// NON-RESPONSIBILITIES:
/// - Event embedding generation (handled by background job)
/// - External integration events (handled by separate handler)
/// </summary>
public class BehaviorLogCreatedEventHandler : IDomainEventHandler<BehaviorLogCreatedEvent>
{
    private readonly IUserInterestScoreRepository _scoreRepo;
    // private readonly IUserEmbeddingRepository _userEmbeddingRepo;
    private readonly IInteractionWeightRepository _weightRepo;

    // private readonly IEventSnapshotRepository _eventSnapshotRepo;
    private readonly IAiUnitOfWork _unitOfWork;
    private readonly ILogger<BehaviorLogCreatedEventHandler> _logger;

    private const double InterestHalfLifeDays = 30.0;

    public BehaviorLogCreatedEventHandler(
        IUserInterestScoreRepository scoreRepo,
        // IUserEmbeddingRepository userEmbeddingRepo,
        IInteractionWeightRepository weightRepo,
        // IEventSnapshotRepository eventSnapshotRepo,
        IAiUnitOfWork aiUnitOfWork,
        ILogger<BehaviorLogCreatedEventHandler> logger)
    {
        _scoreRepo = scoreRepo;
        // _userEmbeddingRepo = userEmbeddingRepo;
        _weightRepo = weightRepo;
        _unitOfWork = aiUnitOfWork;
        // _eventSnapshotRepo = eventSnapshotRepo;
        _logger = logger;
    }

    public async Task Handle(BehaviorLogCreatedEvent @event, CancellationToken ct)
    {
        try
        {
            var metadataHelper = new MetadataHelper(
                @event.Metadata ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>())
            );
            var categories = metadataHelper.GetList(
                keys: new[] { "categories", "category" },  // Check both plural and singular
                delimiters: new[] { ',', ';', '|' }         // Supported delimiters
            );

            if (!categories.Any())
            {
                _logger.LogDebug(
                    "No categories found in metadata for LogId={LogId}, UserId={UserId}",
                    @event.LogId, @event.UserId);
                return;
            }

            var weightEntity = await _weightRepo.GetByActionTypeAsync(@event.ActionType);
            var points = weightEntity?.Weight ?? 1.0;

            var categoryWeights = categories.ToDictionary(c => c, _ => points);
            await _scoreRepo.BulkUpsertWithDecayAsync(
                userId: @event.UserId,
                categoryWeights: categoryWeights,
                halfLifeDays: InterestHalfLifeDays,
                ct: ct
            );
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Interest scores updated: UserId={UserId}, Action={Action}, Categories={Categories}, Points={Points}",
                @event.UserId, @event.ActionType, string.Join(",", categories), points);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process interest scoring for LogId={LogId}, UserId={UserId}",
                @event.LogId, @event.UserId);

        }
    }

    /// <summary>
    /// Marks the user's embedding as stale so it gets rebuilt in the next scheduled job.
    /// This is a lightweight operation — just a flag update.
    /// </summary>
    // private async Task MarkUserEmbeddingStaleAsync(Guid userId, CancellationToken ct)
    // {
    //     var userEmbedding = await _userEmbeddingRepo.GetByUserIdAsync(userId, ct);
    //     if (userEmbedding != null && !userEmbedding.IsStale)
    //     {
    //         userEmbedding.MarkStale();
    //         await _unitOfWork.SaveChangesAsync(ct);
    //         _logger.LogDebug("Marked UserEmbedding as stale for UserId={UserId}", userId);
    //     }
    // }
}