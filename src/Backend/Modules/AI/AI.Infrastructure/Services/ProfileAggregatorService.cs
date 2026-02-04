using AI.Application.Abstractions;
using AI.Domain.Entities;
using AI.Domain.Repositories;
// Required packages (to install later):
// - Microsoft.ML
// - Microsoft.ML.Data
// - Microsoft.ML.LightGBM (for gradient boosting)

public sealed class ProfileAggregatorService : IProfileAggregatorService
{
    private readonly IUserBehaviorLogRepository _logRepo;
    private readonly IUserInterestScoreRepository _scoreRepo;
    private readonly IInteractionWeightRepository _weights;
    // Pretend we have an event repo for category resolution
    // private readonly IEventRepository _eventRepo;

    // ML.NET context and models (commented until installed)
    // private readonly MLContext _mlContext;
    // private readonly ITransformer _linearModel;
    // private readonly ITransformer _gbmModel;
    // private readonly PredictionEngine<UserActionData, UserActionPrediction> _linearEngine;
    // private readonly PredictionEngine<UserActionData, UserActionPrediction> _gbmEngine;

    public ProfileAggregatorService(
        IUserBehaviorLogRepository logRepo,
        IUserInterestScoreRepository scoreRepo,
        IInteractionWeightRepository weights
        //,
        // IEventRepository eventRepo,
        // MLContext mlContext,
        // ITransformer linearModel,
        // ITransformer gbmModel
        )
    {
        _logRepo = logRepo;
        _scoreRepo = scoreRepo;
        _weights = weights;
        // _eventRepo = eventRepo;

        // _mlContext = mlContext;
        // _linearModel = linearModel;
        // _gbmModel = gbmModel;
        // _linearEngine = _mlContext.Model.CreatePredictionEngine<UserActionData, UserActionPrediction>(_linearModel);
        // _gbmEngine = _mlContext.Model.CreatePredictionEngine<UserActionData, UserActionPrediction>(_gbmModel);
    }

    public async Task AggregateUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var logs = await _logRepo.GetByUserAsync(userId, cancellationToken);

        foreach (var log in logs)
        {
            // Manual weight (baseline)
            var weightEntity = await _weights.GetByActionTypeAsync(log.ActionType, cancellationToken);
            var weight = weightEntity?.Weight ?? 0;

            // Resolve category (placeholder)
            // var categoryId = await _eventRepo.GetCategoryByEventIdAsync(log.EventId, cancellationToken);
            var categoryId = 1;

            var score = await _scoreRepo.GetByUserAndCategoryAsync(userId, categoryId, cancellationToken);
            if (score is null)
            {
                score = UserInterestScore.Create(userId, categoryId, 0);
                _scoreRepo.Add(score);
            }

            // --- OPTION 1: Manual heuristic weight ---
            score.UpdateScore(weight);

            // --- OPTION 2: Linear model (logistic regression) ---
            // var sample = new UserActionData { Views = log.ActionType == "View" ? 1 : 0,
            //                                   Clicks = log.ActionType == "Click" ? 1 : 0,
            //                                   Purchases = log.ActionType == "Purchase" ? 1 : 0 };
            // var prediction = _linearEngine.Predict(sample);
            // score.UpdateScore(prediction.Probability);

            // --- OPTION 3: Gradient Boosted Trees (LightGBM) ---
            // var gbmPrediction = _gbmEngine.Predict(sample);
            // score.UpdateScore(gbmPrediction.Probability);

            // Apply time-based exponential decay
            var daysSinceLast = (DateTime.UtcNow - score.LastInteractionAt).TotalDays;
            var decayFactor = Math.Exp(-0.05 * daysSinceLast); // λ = 0.05
            score.ApplyDecay(decayFactor);

            _scoreRepo.Update(score);
        }
    }

    public async Task ApplyDecayAsync(Guid userId, double decayFactor, CancellationToken cancellationToken = default)
    {
        var scores = await _scoreRepo.GetByUserAsync(userId, cancellationToken);

        foreach (var score in scores)
        {
            score.ApplyDecay(decayFactor);
            _scoreRepo.Update(score);
        }
    }
}

// Example ML.NET data classes (commented until package installed)
// public class UserActionData
// {
//     public float Views { get; set; }
//     public float Clicks { get; set; }
//     public float Purchases { get; set; }
//     public bool Label { get; set; } // Did the user engage?
// }

// public class UserActionPrediction
// {
//     public bool PredictedLabel { get; set; }
//     public float Probability { get; set; }
// }
