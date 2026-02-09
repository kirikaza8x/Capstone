using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AI.Domain.Services;
using Microsoft.Extensions.Logging; // 1. Added Namespace

namespace AI.Application.Services
{
    public class UserActivityOrchestrator : IUserActivityOrchestrator
    {
        private readonly IUserBehaviorLogRepository _logRepo;
        private readonly IUserInterestScoreRepository _scoreRepo;
        private readonly InteractionWeightCalculator _weightCalculator;
        private readonly IAiUnitOfWork _unitOfWork;
        private readonly ILogger<UserActivityOrchestrator> _logger; // 2. Added Logger Field

        private const double DefaultDecayHalfLifeInDays = 7.0;

        public UserActivityOrchestrator(
            IUserBehaviorLogRepository logRepo,
            IUserInterestScoreRepository scoreRepo,
            InteractionWeightCalculator weightCalculator,
            IAiUnitOfWork unitOfWork,
            ILogger<UserActivityOrchestrator> logger) // 3. Injected Logger
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
            // LOG ENTRY
            _logger.LogInformation("Starting AI Tracking | User: {UserId} | Action: {ActionType} | Target: {TargetId}", userId, actionType, targetId);

            try 
            {
                // ---------------------------------------------------------
                // STEP 1: LOGGING
                // ---------------------------------------------------------
                var log = UserBehaviorLog.Create(userId, actionType, targetId, targetType, metadata);
                _logRepo.Add(log); 
                
                _logger.LogDebug("Audit Log added to memory.");

                // ---------------------------------------------------------
                // STEP 2: PARSING
                // ---------------------------------------------------------
                var categories = log.GetCategories();
                
                if (categories != null && categories.Any())
                {
                    _logger.LogInformation("Categories identified: {Categories}", string.Join(", ", categories));

                    // ---------------------------------------------------------
                    // STEP 3: CALCULATE VALUE
                    // ---------------------------------------------------------
                    double actionWeight = await _weightCalculator.CalculateWeightAsync(userId, actionType);
                    
                    _logger.LogDebug("Calculated Weight for '{Action}': {Weight}", actionType, actionWeight);

                    // ---------------------------------------------------------
                    // STEP 4: UPDATE SCORES
                    // ---------------------------------------------------------
                    foreach (var category in categories)
                    {
                        var userScore = await _scoreRepo.GetByUserAndCategoryAsync(userId, category);

                        if (userScore == null)
                        {
                            _logger.LogInformation("New Interest detected: {Category}", category);
                            userScore = UserInterestScore.Create(userId, category, actionWeight);
                            _scoreRepo.Add(userScore);
                        }
                        else
                        {
                            _logger.LogDebug("Updating existing Interest: {Category}", category);
                            userScore.ApplyDecay(DefaultDecayHalfLifeInDays); 
                            userScore.AddScore(actionWeight);
                            _scoreRepo.Update(userScore);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("No categories found in metadata for {TargetType}. Skipping score update.", targetType);
                }

                // ---------------------------------------------------------
                // STEP 5: SAVE CHANGES
                // ---------------------------------------------------------
                _logger.LogInformation("Saving changes to database...");
                
                await _unitOfWork.SaveChangesAsync(); 
                
                _logger.LogInformation("AI Tracking completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL FAILURE in AI Tracking for User {UserId}", userId);
                throw; 
            }
        }
    }
}