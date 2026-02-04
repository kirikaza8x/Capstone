// File: AI.Application/Services/MLModelTrainingService.cs (COMMENTED - Use when data ready)
/*
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.LightGbm;
using AI.Domain.Repositories;

namespace AI.Application.Services
{
    /// <summary>
    /// ML MODEL TRAINING SERVICE (COMMENTED UNTIL DATA READY)
    /// 
    /// ENABLE THIS when you have:
    /// - 1000+ UserBehaviorLog records
    /// - LedToConversion field populated (from online learning)
    /// 
    /// Purpose:
    /// - Train LightGBM model on historical behavior data
    /// - Predict which actions are likely to lead to conversions
    /// - Run periodically (daily/weekly) to retrain with new data
    /// 
    /// ALGORITHM: LightGBM Gradient Boosting
    /// - Ensemble of decision trees
    /// - Learns which action patterns lead to conversions
    /// - Fast training and prediction
    /// </summary>
    public class MLModelTrainingService
    {
        private readonly MLContext _mlContext;
        private readonly IUserBehaviorLogRepository _logRepo;

        public MLModelTrainingService(IUserBehaviorLogRepository logRepo)
        {
            _mlContext = new MLContext(seed: 42); // Fixed seed for reproducibility
            _logRepo = logRepo;
        }

        /// <summary>
        /// Train LightGBM model on historical user behavior
        /// 
        /// Steps:
        /// 1. Collect all behavior logs with conversion labels
        /// 2. Engineer features (same as prediction features)
        /// 3. Split into train (80%) and test (20%)
        /// 4. Train LightGBM model
        /// 5. Evaluate performance
        /// 6. Save model to disk
        /// 
        /// Returns: Trained model transformer
        /// </summary>
        public async Task<ITransformer> TrainModelAsync(CancellationToken cancellationToken)
        {
            // Step 1: Collect training data
            // Need to get logs with conversion labels filled in
            var allLogs = await _logRepo.GetAllWithConversionLabelsAsync(cancellationToken);
            
            if (allLogs.Count < 1000)
            {
                throw new InvalidOperationException(
                    $"Not enough training data. Have {allLogs.Count}, need at least 1000 labeled interactions.");
            }
            
            // Step 2: Feature engineering
            var trainingData = allLogs.Select(log => new UserActionFeatures
            {
                ActionType = log.ActionType,
                HoursSinceAction = (float)(DateTime.UtcNow - log.CreatedAt).TotalHours,
                // Note: Would need to add these fields to UserBehaviorLog or calculate from history
                // UserTotalActions = log.UserTotalActionsAtTime,
                // UserActionsOfThisType = log.UserActionsOfThisTypeAtTime,
                // SessionActionCount = log.SessionActionCountAtTime,
                DayOfWeek = (int)log.CreatedAt.DayOfWeek,
                HourOfDay = log.CreatedAt.Hour,
                // ActionFrequencyRatio = log.ActionFrequencyRatioAtTime,
                // UserEngagementDiversity = log.UserEngagementDiversityAtTime,
                Label = log.LedToConversion // This is our target!
            }).ToList();

            // Convert to ML.NET's IDataView
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Step 3: Train/test split
            var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Step 4: Define ML pipeline
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ActionTypeEncoded", "ActionType")
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "ActionTypeEncoded",
                    "HoursSinceAction",
                    "DayOfWeek",
                    "HourOfDay"))
                .Append(_mlContext.BinaryClassification.Trainers.LightGbm(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    numberOfLeaves: 31,              // Tree complexity
                    minimumExampleCountPerLeaf: 20,  // Prevent overfitting
                    learningRate: 0.1,                // How fast to learn
                    numberOfIterations: 100));        // Number of trees

            // Step 5: Train the model
            Console.WriteLine("Training LightGBM model...");
            var model = pipeline.Fit(trainTestSplit.TrainSet);

            // Step 6: Evaluate on test set
            var predictions = model.Transform(trainTestSplit.TestSet);
            var metrics = _mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label");

            Console.WriteLine($"Model Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");

            // Step 7: Save model to disk
            _mlContext.Model.Save(model, dataView.Schema, "recommendation_model.zip");
            Console.WriteLine("Model saved to recommendation_model.zip");

            return model;
        }

        /// <summary>
        /// Load pre-trained model from disk
        /// </summary>
        public ITransformer LoadModel(string modelPath = "recommendation_model.zip")
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"Model file not found: {modelPath}");
            }
            
            return _mlContext.Model.Load(modelPath, out var modelSchema);
        }
    }

    // ===== ML.NET DATA CLASSES =====
    
    /// <summary>
    /// Features for ML model input
    /// Used by LightGBM for binary classification (will action lead to conversion?)
    /// </summary>
    public class UserActionFeatures
    {
        [LoadColumn(0)]
        public string ActionType { get; set; } = default!;
        
        [LoadColumn(1)]
        public float HoursSinceAction { get; set; }
        
        [LoadColumn(2)]
        public float UserTotalActions { get; set; }
        
        [LoadColumn(3)]
        public float UserActionsOfThisType { get; set; }
        
        [LoadColumn(4)]
        public float SessionActionCount { get; set; }
        
        [LoadColumn(5)]
        public int DayOfWeek { get; set; }
        
        [LoadColumn(6)]
        public int HourOfDay { get; set; }
        
        [LoadColumn(7)]
        public float ActionFrequencyRatio { get; set; }
        
        [LoadColumn(8)]
        public int UserEngagementDiversity { get; set; }
        
        [LoadColumn(9)]
        public bool Label { get; set; } // Training target: did it lead to conversion?
    }

    /// <summary>
    /// ML model output prediction
    /// </summary>
    public class ActionScorePrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }
        
        [ColumnName("Probability")]
        public float Probability { get; set; } // Main score we use (0-1)
        
        [ColumnName("Score")]
        public float Score { get; set; } // Raw model score before sigmoid
    }
}
*/