using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AI.Domain.Services;

namespace AI.Application.Services
{
    public class UserActivityOrchestrator
    {
        private readonly IUserBehaviorLogRepository _logRepo;
        private readonly IUserInterestScoreRepository _scoreRepo;
        private readonly InteractionWeightCalculator _weightCalculator;
        private readonly IAiUnitOfWork _unitOfWork;

        // Configuration constant (could be moved to appsettings later)
        private const double DefaultDecayHalfLifeInDays = 7.0;

        public UserActivityOrchestrator(
            IUserBehaviorLogRepository logRepo,
            IUserInterestScoreRepository scoreRepo,
            InteractionWeightCalculator weightCalculator,
            IAiUnitOfWork unitOfWork)
        {
            _logRepo = logRepo;
            _scoreRepo = scoreRepo;
            _weightCalculator = weightCalculator;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleUserActivityAsync(
            Guid userId, 
            string actionType, 
            string targetId, 
            string targetType, 
            Dictionary<string, string>? metadata)
        {
            // Use a specific transaction scope to ensure rollback on error
            await _unitOfWork.BeginTransactionAsync();

            try 
            {
                // ---------------------------------------------------------
                // STEP 1: LOGGING (Audit Trail)
                // ---------------------------------------------------------
                var log = UserBehaviorLog.Create(userId, actionType, targetId, targetType, metadata);
                _logRepo.Add(log); // Adds to EF ChangeTracker (Memory)

                // ---------------------------------------------------------
                // STEP 2: PARSING & GUARD CLAUSE
                // ---------------------------------------------------------
                var categories = log.GetCategories();

                // CRITICAL FIX: 
                // If there are no categories, we still want to save the LOG (Audit Trail).
                // So we don't 'return' yet; we just skip the scoring part.
                if (categories.Any())
                {
                    // ---------------------------------------------------------
                    // STEP 3: CALCULATE VALUE
                    // ---------------------------------------------------------
                    double actionWeight = await _weightCalculator.CalculateWeightAsync(userId, actionType);

                    // ---------------------------------------------------------
                    // STEP 4: UPDATE SCORES
                    // ---------------------------------------------------------
                    foreach (var category in categories)
                    {
                        var userScore = await _scoreRepo.GetAsync(userId, category);

                        if (userScore == null)
                        {
                            // New Interest
                            userScore = UserInterestScore.Create(userId, category, actionWeight);
                            _scoreRepo.Add(userScore);
                        }
                        else
                        {
                            // Existing Interest: Decay -> Update
                            userScore.ApplyDecay(DefaultDecayHalfLifeInDays); 
                            userScore.AddScore(actionWeight);
                            _scoreRepo.Update(userScore);
                        }
                    }
                }

                // ---------------------------------------------------------
                // STEP 5: COMMIT (The Save Point)
                // ---------------------------------------------------------
                // CRITICAL FIX: This saves both the Log AND the Scores in one go.
                await _unitOfWork.CommitTransactionAsync(); 
                
                // If your UnitOfWork doesn't auto-commit the transaction, do it here:
                // await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                //  _logger.LogError(ex, "Failed to process user activity");
                
                // Rollback happens automatically when 'using' block ends, 
                // or you can call it explicitly:
                await _unitOfWork.RollbackTransactionAsync();
                
                throw; // Optional: Rethrow if you want the API to return 500 Error
            }
        }
    }
}