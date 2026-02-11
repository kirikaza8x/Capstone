using AI.Application.Services;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AI.Domain.Services;
using Microsoft.Extensions.Logging;

namespace AI.Application.Abstractions
{
    /// <summary>
    /// Orchestrates the complete user activity tracking workflow.
    /// CRITICAL PATH: This is the main entry point for all user behavior tracking.
    ///
    /// WORKFLOW (Master Plan Steps 1-4):
    /// 1. Log the raw action (immutable audit trail)
    /// 2. Parse categories from metadata
    /// 3. Calculate action weight (global or personalized)
    /// 4. Update user interest scores with decay + new points
    ///
    /// OPTIMIZATIONS IMPLEMENTED:
    /// - ExecutionStrategy-safe transaction wrapping
    /// - Batch fetching for multiple categories
    /// - Atomic persistence
    /// - Comprehensive logging
    /// </summary>
    public class UserActivityOrchestrator : IUserActivityOrchestrator
    {
        private readonly IUserBehaviorLogRepository _logRepo;
        private readonly IUserInterestScoreRepository _scoreRepo;
        private readonly InteractionWeightCalculator _weightCalculator;
        private readonly IAiUnitOfWork _unitOfWork;
        private readonly ILogger<UserActivityOrchestrator> _logger;

        private const double DEFAULT_DECAY_HALF_LIFE_IN_DAYS = 7.0;
        private const int MAX_CATEGORIES_PER_ACTION = 10;

        public UserActivityOrchestrator(
            IUserBehaviorLogRepository logRepo,
            IUserInterestScoreRepository scoreRepo,
            InteractionWeightCalculator weightCalculator,
            IAiUnitOfWork unitOfWork,
            ILogger<UserActivityOrchestrator> logger)
        {
            _logRepo = logRepo;
            _scoreRepo = scoreRepo;
            _weightCalculator = weightCalculator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleUserActivityAsync(
            Guid userId,
            string actionType,
            string targetId,
            string targetType,
            Dictionary<string, string>? metadata)
        {
            _logger.LogInformation(
                "Tracking Started | User: {UserId} | Action: {ActionType} | Target: {TargetId} ({TargetType})",
                userId, actionType, targetId, targetType);

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // ---------------------------------------------------------
                    // STEP 1: IMMUTABLE AUDIT LOG
                    // ---------------------------------------------------------
                    var log = UserBehaviorLog.Create(
                        userId,
                        actionType,
                        targetId,
                        targetType,
                        metadata);

                    _logRepo.Add(log);

                    _logger.LogDebug("Audit log created: {LogId}", log.Id);

                    // ---------------------------------------------------------
                    // STEP 2: PARSE CATEGORIES
                    // ---------------------------------------------------------
                    var categories = log.GetCategories();

                    if (categories == null || !categories.Any())
                    {
                        _logger.LogWarning(
                            "No categories found in metadata for {TargetType}. Saving log only.",
                            targetType);

                        return; // log still persisted (transaction will commit)
                    }

                    if (categories.Count > MAX_CATEGORIES_PER_ACTION)
                    {
                        _logger.LogWarning(
                            "Too many categories ({Count}). Truncating to {Max}.",
                            categories.Count, MAX_CATEGORIES_PER_ACTION);

                        categories = categories
                            .Take(MAX_CATEGORIES_PER_ACTION)
                            .ToList();
                    }

                    _logger.LogInformation(
                        "Categories identified: {Categories}",
                        string.Join(", ", categories));

                    // ---------------------------------------------------------
                    // STEP 3: CALCULATE ACTION WEIGHT
                    // ---------------------------------------------------------
                    double actionWeight =
                        await _weightCalculator.CalculateWeightAsync(userId, actionType);

                    _logger.LogDebug(
                        "Calculated weight for '{Action}': {Weight}",
                        actionType, actionWeight);

                    // ---------------------------------------------------------
                    // STEP 4: UPDATE INTEREST SCORES (BATCHED)
                    // ---------------------------------------------------------
                    var existingScores =
                        await _scoreRepo.GetByUserAndCategoriesAsync(userId, categories);

                    var existingDict =
                        existingScores.ToDictionary(s => s.Category);

                    int newInterests = 0;
                    int updatedInterests = 0;

                    foreach (var category in categories)
                    {
                        if (existingDict.TryGetValue(category, out var userScore))
                        {
                            userScore.ApplyDecay(DEFAULT_DECAY_HALF_LIFE_IN_DAYS);
                            userScore.AddScore(actionWeight);
                            _scoreRepo.Update(userScore);

                            updatedInterests++;
                        }
                        else
                        {
                            var newScore =
                                UserInterestScore.Create(userId, category, actionWeight);

                            _scoreRepo.Add(newScore);
                            newInterests++;
                        }
                    }

                    _logger.LogInformation(
                        "Tracking staged | New: {New} | Updated: {Updated} | Total: {Total}",
                        newInterests, updatedInterests, categories.Count);
                });

                _logger.LogInformation(
                    "Tracking completed successfully | User: {UserId} | Action: {ActionType}",
                    userId, actionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CRITICAL FAILURE in AI Tracking for User {UserId} | Action: {ActionType}",
                    userId, actionType);

                throw;
            }
        }
    }
}
