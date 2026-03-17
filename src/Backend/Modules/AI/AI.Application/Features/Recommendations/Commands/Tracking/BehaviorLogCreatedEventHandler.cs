// AI.Application/Features/Tracking/EventHandlers/BehaviorLogCreatedEventHandler.cs

using AI.Domain.Events;
using AI.Domain.Repositories;
using AI.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using AI.Domain.Interfaces.UOW;
using System.Collections.ObjectModel;

namespace AI.Application.Features.Tracking.EventHandlers;

public class BehaviorLogCreatedEventHandler : IDomainEventHandler<BehaviorLogCreatedEvent>
{
    private readonly IUserInterestScoreRepository _scoreRepo;
    private readonly IInteractionWeightRepository _weightRepo;
    private readonly IAiUnitOfWork _unitOfWork;
    private readonly ILogger<BehaviorLogCreatedEventHandler> _logger;

    private const double InterestHalfLifeDays = 30.0;

    public BehaviorLogCreatedEventHandler(
        IUserInterestScoreRepository scoreRepo,
        IInteractionWeightRepository weightRepo,
        IAiUnitOfWork aiUnitOfWork,
        ILogger<BehaviorLogCreatedEventHandler> logger)
    {
        _scoreRepo = scoreRepo;
        _weightRepo = weightRepo;
        _unitOfWork = aiUnitOfWork;
        _logger = logger;
    }

    public async Task Handle(BehaviorLogCreatedEvent @event, CancellationToken ct)
    {
        try
        {
            // Extract categories using your helper
            var metadataHelper = new MetadataHelper(
                @event.Metadata ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>())
            );
            var categories = metadataHelper.GetList(
                keys: new[] { "categories", "category" },
                delimiters: new[] { ',', ';', '|' }
            );

            if (!categories.Any())
            {
                _logger.LogDebug(
                    "No categories found in metadata for LogId={LogId}, UserId={UserId}",
                    @event.LogId, @event.UserId);
                return;
            }

            // Update interest scores
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

            //  Mark user profile as stale in Qdrant (triggers rebuild)

            _logger.LogInformation(
                "Updated interest scores + marked profile stale: UserId={UserId}, Categories={Categories}",
                @event.UserId, string.Join(",", categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process interest scoring for LogId={LogId}, UserId={UserId}",
                @event.LogId, @event.UserId);
            // Don't rethrow — scoring is a side-effect
        }
    }

    /// <summary>
    /// Marks the user's Qdrant profile as stale so it gets rebuilt in the next scheduled job.
    /// </summary>
   
}