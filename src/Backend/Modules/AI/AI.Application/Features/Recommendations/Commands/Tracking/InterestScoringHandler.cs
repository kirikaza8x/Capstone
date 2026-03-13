using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using Shared.Application.Abstractions.EventBus;
using Shared.IntegrationEvents.AI;

public class InterestScoringHandler
    : IntegrationEventHandler<TrackUserActivityIntegrationEvent>
{
    private readonly IUserInterestScoreRepository _repository;
    private readonly IInteractionWeightRepository _weightRepository;

    private readonly IUserEmbeddingRepository _userEmbeddingRepository;
    private readonly IAiUnitOfWork _uow;

    public InterestScoringHandler(
        IUserInterestScoreRepository repository,
        IInteractionWeightRepository weightRepository,
        IUserEmbeddingRepository userEmbeddingRepository,
        IAiUnitOfWork uow
        )
    {
        _repository = repository;
        _weightRepository = weightRepository;
        _userEmbeddingRepository = userEmbeddingRepository;
        _uow = uow;
    }

    public override async Task Handle(
        TrackUserActivityIntegrationEvent e,
        CancellationToken ct)
    {
        if (e.Metadata == null ||
            !e.Metadata.TryGetValue("category", out var category))
            return;

        var weights = await _weightRepository
            .GetAllActiveWeightsAsync("default", ct);

        if (!weights.TryGetValue(e.ActionType, out var points))
            points = 1;

        await _repository.UpsertAsync(
            e.UserId,
            category,
            points,
            halfLifeDays: 30,
            ct);

        var userProfile = await _userEmbeddingRepository.GetByUserIdAsync(e.UserId, ct);
        if (userProfile != null)
        {
            userProfile.MarkStale();
            _userEmbeddingRepository.Update(userProfile);
        }

        await _uow.SaveChangesAsync(ct);
    }
}