using AI.Domain.Events;
using AI.Domain.Helpers;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Tracking.EventHandlers;

/// <summary>
/// Handles BehaviorLogCreatedEvent — updates UserInterestScore per category.
///
/// SAVE RESPONSIBILITY: repo only stages changes, UoW commits here.
/// RUNS ALONGSIDE BehaviorLogEmbeddingHandler — MediatR dispatches both.
/// </summary>
public sealed class BehaviorLogCreatedEventHandler(
    IUserInterestScoreRepository            scoreRepo,
    IInteractionWeightRepository            weightRepo,
    IAiUnitOfWork                           unitOfWork,
    ILogger<BehaviorLogCreatedEventHandler> logger)
    : IDomainEventHandler<BehaviorLogCreatedEvent>
{
    private const double InterestHalfLifeDays = 30.0;

    public async Task Handle(BehaviorLogCreatedEvent @event, CancellationToken ct)
    {
        try
        {
            var categories = new MetadataHelper(
                    @event.Metadata ?? new Dictionary<string, string>())
                .GetList(new[] { "categories", "category" });

            if (categories.Count == 0)
            {
                logger.LogDebug(
                    "No categories in metadata — skipping interest score update. LogId={LogId}",
                    @event.LogId);
                return;
            }

            // Falls back to 1.0 if action type not seeded in InteractionWeight yet
            var weightEntity = await weightRepo.GetByActionTypeAsync(@event.ActionType, ct: ct);
            var points       = weightEntity?.Weight ?? 1.0;

            // Stages changes in EF change tracker — does NOT save
            await scoreRepo.BulkUpsertWithDecayAsync(
                userId:          @event.UserId,
                categoryWeights: categories.ToDictionary(c => c, _ => points),
                halfLifeDays:    InterestHalfLifeDays,
                ct:              ct
            );

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation(
                "Interest scores updated — UserId={UserId}, Action={Action}, " +
                "Categories=[{Categories}], Points={Points}",
                @event.UserId, @event.ActionType,
                string.Join(", ", categories), points);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to update interest scores. LogId={LogId}, UserId={UserId}",
                @event.LogId, @event.UserId);
        }
    }
}