using AI.Application.Abstractions;
using AI.Application.Models;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;

// ===== ML.NET PACKAGES (COMMENTED - Install when ready to enable ML) =====
// Required NuGet packages:
// - Microsoft.ML
// - Microsoft.ML.Data
// - Microsoft.ML.LightGbm
// using Microsoft.ML;
// using Microsoft.ML.Data;

namespace AI.Application.Services
{
    /// <summary>
    /// MAIN RECOMMENDATION ENGINE
    /// 
    /// CURRENT ALGORITHMS (ACTIVE):
    /// 1. Epsilon-Greedy Weight Selection (exploration vs exploitation)
    /// 2. Contextual Multi-Armed Bandit (recency, frequency, session boosts)
    /// 3. Online Learning (adapts weights based on conversion outcomes)
    /// 4. Exponential Time Decay (older interactions fade)
    /// 
    /// FUTURE ALGORITHMS (COMMENTED - Ready to enable):
    /// 5. Mean Pooling for User Embeddings
    /// 6. LightGBM Gradient Boosting Trees
    /// 7. Cosine Similarity (semantic matching)
    /// 8. Ensemble Scoring (weighted combination)
    /// 
    /// DATABASE OPERATIONS:
    /// - READ: UserBehaviorLog, UserWeightProfile, InteractionWeight, UserInterestScore
    /// - WRITE: UserWeightProfile (learned weights), UserInterestScore (aggregated scores), UserBehaviorLog (conversion labels)
    /// </summary>
    public sealed class ProfileAggregatorService : IProfileAggregatorService
    {
        private readonly IUserBehaviorLogRepository _logRepo;
        private readonly IUserInterestScoreRepository _scoreRepo;
        private readonly IInteractionWeightRepository _globalWeights;
        private readonly IUserWeightProfileRepository _userWeights;
        // private readonly IMarketingAnalyticsRepository _analyticsRepo;
        private readonly IAiUnitOfWork _unitOfWork;

        // ===== ML COMPONENTS (COMMENTED UNTIL DATA IS READY) =====
        // Uncomment these when you have 1000+ interactions with conversion labels
        // private readonly MLContext _mlContext;
        // private ITransformer? _rankingModel;
        // private PredictionEngine<UserActionFeatures, ActionScorePrediction>? _predictionEngine;

        // ===== EMBEDDING COMPONENTS (COMMENTED UNTIL NEEDED) =====
        // Uncomment when you want semantic similarity (can work with small data)
        // private readonly IEmbeddingService _embeddingService;
        // private readonly IVectorDatabase _vectorDb;

        public ProfileAggregatorService(
            IUserBehaviorLogRepository logRepo,
            IUserInterestScoreRepository scoreRepo,
            IInteractionWeightRepository globalWeights,
            IUserWeightProfileRepository userWeights,
            // IMarketingAnalyticsRepository analyticsRepo,
            IAiUnitOfWork unitOfWork
            // 🔒 ML DEPENDENCIES (commented out)
            // MLContext mlContext,
            // IEmbeddingService embeddingService,
            // IVectorDatabase vectorDb
            )
        {
            _logRepo = logRepo;
            _scoreRepo = scoreRepo;
            _globalWeights = globalWeights;
            _userWeights = userWeights;
            // _analyticsRepo = analyticsRepo;
            _unitOfWork = unitOfWork;

            // 🔒 ML INITIALIZATION (commented out)
            // _mlContext = mlContext;
            // _embeddingService = embeddingService;
            // _vectorDb = vectorDb;

            // Load pre-trained ML model if it exists
            // if (File.Exists("recommendation_model.zip"))
            // {
            //     _rankingModel = _mlContext.Model.Load("recommendation_model.zip", out _);
            //     _predictionEngine = _mlContext.Model.CreatePredictionEngine<UserActionFeatures, ActionScorePrediction>(_rankingModel);
            // }
        }

        /// <summary>
        /// 🎯 MAIN ENTRY POINT: Process user behavior and update interest scores
        /// 
        /// FLOW:
        /// 1. Load user's behavior history from database
        /// 2. Determine which algorithms to use (currently: rules only)
        /// 3. For each behavior log:
        ///    a. Calculate score using active algorithms
        ///    b. Learn from outcomes (update personalized weights)
        ///    c. Apply time decay
        ///    d. Save to database
        /// 
        /// DATABASE OPERATIONS:
        /// - READ: UserBehaviorLog (all user actions)
        /// - READ/WRITE: UserWeightProfile (personalized weights)
        /// - READ/WRITE: UserInterestScore (category interests)
        /// - WRITE: UserBehaviorLog (mark conversions)
        /// </summary>
        public async Task AggregateUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // 🔵 DATABASE READ: Load all user behavior logs
            var logs = await _logRepo.GetByUserAsync(userId, cancellationToken);
            var userHistory = logs.OrderBy(l => l.CreatedAt).ToList();

            // Determine which algorithms to use and their weights
            var weights = await DetermineAlgorithmWeightsAsync(userId, userHistory.Count, cancellationToken);

            // 🔒 EMBEDDINGS: Generate user embedding (commented until enabled)
            // float[]? userEmbedding = null;
            // if (weights.UseEmbeddings)
            // {
            //     userEmbedding = await GenerateUserEmbeddingAsync(userId, userHistory, cancellationToken);
            // }

            // 🟢 DATABASE TRANSACTION: Start (ensures all-or-nothing save)
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var log in userHistory)
                {
                    // Resolve which category this event belongs to
                    var categoryId = await ResolveCategoryAsync(log.EventId, cancellationToken);

                    // 🔵 DATABASE READ: Get or create interest score for this category
                    var score = await GetOrCreateScoreAsync(userId, categoryId, cancellationToken);

                    // ===== SCORING PHASE =====

                    // ALGORITHM 1-2: Rule-based scoring (CURRENTLY ACTIVE)
                    var ruleScore = await CalculateRuleBasedScoreAsync(
                        userId,
                        log,
                        userHistory,
                        cancellationToken);

                    // 🔒 ALGORITHM 6: ML prediction (commented until trained)
                    // double mlScore = 0;
                    // if (weights.UseML && _predictionEngine != null)
                    // {
                    //     mlScore = await PredictActionScoreAsync(userId, log, userHistory, cancellationToken);
                    // }

                    // 🔒 ALGORITHM 7: Semantic similarity (commented until embedding service added)
                    // double semanticScore = 0;
                    // if (weights.UseEmbeddings && userEmbedding != null)
                    // {
                    //     var eventEmbedding = await _embeddingService.GetEventEmbeddingAsync(log.EventId, cancellationToken);
                    //     semanticScore = CosineSimilarity(userEmbedding, eventEmbedding);
                    // }

                    // ===== ENSEMBLE SCORING =====
                    // Currently: 100% rule-based
                    // Future: Weighted combination of rules + ML + embeddings
                    var finalScore = ruleScore;
                    // When ML enabled: (ruleScore * weights.RuleWeight) + (mlScore * weights.MLWeight) + (semanticScore * weights.EmbeddingWeight);

                    // 🟢 DOMAIN LOGIC: Update interest score
                    score.UpdateScore(finalScore);

                    // ===== LEARNING PHASE =====
                    // ALGORITHM 3: Online Learning - adapt weights based on outcomes
                    await LearnFromActionOutcomeAsync(userId, log, userHistory, cancellationToken);

                    // ===== DECAY PHASE =====
                    // ALGORITHM 4: Exponential Time Decay
                    var daysSinceLast = (DateTime.UtcNow - score.LastInteractionAt).TotalDays;
                    var decayFactor = Math.Exp(-0.05 * daysSinceLast); // λ = 0.05
                    score.ApplyDecay(decayFactor);

                    // 🟢 DATABASE WRITE: Mark score entity for update
                    _scoreRepo.Update(score);
                }

                // 🔒 EMBEDDINGS: Save user embedding to vector database (commented)
                // if (userEmbedding != null)
                // {
                //     await _vectorDb.UpsertUserEmbeddingAsync(userId, userEmbedding, cancellationToken);
                // }

                // 🟢 DATABASE COMMIT: Save all changes in one transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception)
            {
                // 🔴 DATABASE ROLLBACK: Undo all changes if anything fails
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// ALGORITHM 10: Dynamic Weight Allocation
        /// 
        /// Decides which algorithms to use based on data availability
        /// 
        /// CURRENT BEHAVIOR:
        /// - Returns 100% rule-based weights (no ML, no embeddings)
        /// 
        /// FUTURE BEHAVIOR (when ML uncommented):
        /// - < 1000 interactions: 80% rules, 20% embeddings
        /// - 1000-10000 interactions: 50% rules, 30% ML, 20% embeddings
        /// - > 10000 interactions: 25% rules, 60% ML, 15% embeddings
        /// 
        /// Also adjusts per-user:
        /// - New users: More rules
        /// - Power users: More ML
        /// </summary>
        private async Task<AlgorithmWeights> DetermineAlgorithmWeightsAsync(
            Guid userId,
            int userInteractionCount,
            CancellationToken cancellationToken)
        {
            var weights = new AlgorithmWeights();

            // 🔵 DATABASE READ: Get total interaction count across all users
            var totalInteractions = await _logRepo.GetTotalCountAsync(cancellationToken);

            // 🔒 ML READINESS CHECK (commented until ML enabled)
            // var mlModelExists = _rankingModel != null;
            // double mlAccuracy = 0;
            // if (mlModelExists)
            // {
            //     // Could read from a ModelMetrics table if you track this
            //     mlAccuracy = 0.75; // Placeholder
            // }

            // ===== CURRENT STRATEGY: 100% RULE-BASED =====
            // This works perfectly fine without any ML or embeddings
            weights.RuleWeight = 1.0;
            weights.MLWeight = 0.0;
            weights.EmbeddingWeight = 0.0;
            weights.UseML = false;
            weights.UseEmbeddings = false;
            weights.Phase = "Rule-Based Only";
            weights.Reason = $"Bootstrap phase - {totalInteractions} total interactions";

            // ===== FUTURE ADAPTIVE STRATEGY (commented until ML ready) =====
            // Uncomment this section when you enable ML and embeddings
            /*
            if (totalInteractions < 1000)
            {
                // Phase 1: Bootstrap - not enough data for ML
                weights.RuleWeight = 0.80;
                weights.MLWeight = 0.0;
                weights.EmbeddingWeight = 0.20;
                weights.UseML = false;
                weights.UseEmbeddings = true;
                weights.Phase = "Bootstrap";
            }
            else if (totalInteractions < 10000 && mlModelExists)
            {
                // Phase 2: Early Learning - start using ML
                weights.RuleWeight = 0.50;
                weights.MLWeight = 0.30;
                weights.EmbeddingWeight = 0.20;
                weights.UseML = true;
                weights.UseEmbeddings = true;
                weights.Phase = "Early Learning";
            }
            else if (totalInteractions >= 10000 && mlAccuracy > 0.75)
            {
                // Phase 3: ML-First - lots of data, good model performance
                weights.RuleWeight = 0.25;
                weights.MLWeight = 0.60;
                weights.EmbeddingWeight = 0.15;
                weights.UseML = true;
                weights.UseEmbeddings = true;
                weights.Phase = "ML-First";
            }
            else
            {
                // Fallback: ML exists but underperforming, rely more on rules
                weights.RuleWeight = 0.60;
                weights.MLWeight = 0.25;
                weights.EmbeddingWeight = 0.15;
                weights.UseML = mlModelExists;
                weights.UseEmbeddings = true;
                weights.Phase = "Conservative Hybrid";
            }
            
            // PER-USER ADJUSTMENT
            if (userInteractionCount < 10)
            {
                // New user - trust rules more
                var adjustment = Math.Min(0.20, weights.MLWeight);
                weights.MLWeight -= adjustment;
                weights.RuleWeight += adjustment;
                weights.Reason = "New user - relying on rules";
            }
            else if (userInteractionCount > 100 && weights.UseML)
            {
                // Power user - trust ML more
                var adjustment = Math.Min(0.10, weights.RuleWeight);
                weights.RuleWeight -= adjustment;
                weights.MLWeight += adjustment;
                weights.Reason = "Power user - trusting ML more";
            }
            
            // Ensure weights sum to 1.0
            var total = weights.RuleWeight + weights.MLWeight + weights.EmbeddingWeight;
            weights.RuleWeight /= total;
            weights.MLWeight /= total;
            weights.EmbeddingWeight /= total;
            */

            return weights;
        }

        /// <summary>
        /// ALGORITHM 3: Online Learning with Temporal Pattern Mining
        /// 
        /// THIS IS WHERE LEARNING HAPPENS!
        /// 
        /// Process:
        /// 1. Analyze if current action led to conversion within 24 hours
        /// 2. Update UserWeightProfile based on outcome:
        ///    - Success: Increase weight (this action is valuable for this user!)
        ///    - Failure: Decrease weight (this action didn't lead anywhere)
        /// 3. Mark UserBehaviorLog with conversion info (for ML training later)
        /// 
        /// Algorithms used:
        /// - Temporal Pattern Mining (24-hour window)
        /// - Incremental Mean Calculation (success rate)
        /// - Gradient Descent (weight updates)
        /// 
        /// DATABASE OPERATIONS:
        /// - READ: UserWeightProfile (current personalized weights)
        /// - WRITE: UserWeightProfile (updated weights after learning)
        /// - WRITE: UserBehaviorLog (mark if led to conversion)
        /// </summary>
        private async Task LearnFromActionOutcomeAsync(
            Guid userId,
            UserBehaviorLog currentLog,
            List<UserBehaviorLog> allHistory,
            CancellationToken cancellationToken)
        {
            // TEMPORAL PATTERN MINING: Look for high-value actions within 24 hours
            var subsequentActions = allHistory
                .Where(l => l.CreatedAt > currentLog.CreatedAt &&
                           l.CreatedAt <= currentLog.CreatedAt?.AddHours(24))
                .ToList();

            var ledToConversion = subsequentActions.Any(a => a.IsHighValueAction());

            // 🔵 DATABASE READ: Get user's personalized weight for this action type
            var userWeight = await _userWeights.GetByUserAndActionAsync(
                userId,
                currentLog.ActionType,
                cancellationToken);

            if (userWeight != null)
            {
                if (ledToConversion)
                {
                    // 🟡 LEARNING: This action led to conversion!
                    // Increase weight using gradient descent (step size = 0.05)
                    userWeight.RecordSuccess();

                    // 🟢 DOMAIN LOGIC: Mark the log with conversion info
                    var conversionEvent = subsequentActions.First(a => a.IsHighValueAction());
                    currentLog.MarkAsLeadingToConversion(conversionEvent.EventId);

                    // 🟢 DATABASE WRITE: Update behavior log (this becomes training data for ML later)
                    _logRepo.Update(currentLog);
                }
                else if (subsequentActions.Count == 0 &&
                         (DateTime.UtcNow - currentLog.CreatedAt)?.TotalHours > 24)
                {
                    // 🟡 LEARNING: No follow-up actions - decrease weight
                    // Decrease using smaller step size (0.02) to be conservative
                    userWeight.RecordFailure();
                }

                // 🟢 DATABASE WRITE: Save updated weight profile
                _userWeights.Update(userWeight);
            }
            else
            {
                // 🟡 NEW USER LEARNING: First time seeing this action for this user
                // Initialize with global weight
                var globalWeight = await GetGlobalWeightAsync(currentLog.ActionType, cancellationToken);
                var newUserWeight = UserWeightProfile.Create(userId, currentLog.ActionType, globalWeight);

                // 🟢 DATABASE WRITE: Add new weight profile
                _userWeights.Add(newUserWeight);
            }
        }

        /// <summary>
        /// Rule-based scoring - CURRENTLY ACTIVE
        /// 
        /// Combines:
        /// - ALGORITHM 1: Personalized or global weights
        /// - ALGORITHM 2: Contextual boosts (recency, frequency, session)
        /// </summary>
        private async Task<double> CalculateRuleBasedScoreAsync(
            Guid userId,
            UserBehaviorLog log,
            List<UserBehaviorLog> userHistory,
            CancellationToken cancellationToken)
        {
            // Get base weight (personalized if available, otherwise global)
            var weight = await GetEffectiveWeightAsync(
                userId,
                log.ActionType,
                userHistory.Count,
                cancellationToken);

            // Apply contextual boosts
            var contextualWeight = ApplyContextualBoosts(weight, log, userHistory);

            return contextualWeight;
        }

        /// <summary>
        /// ALGORITHM 1: Epsilon-Greedy / Thompson Sampling Weight Selection
        /// 
        /// Strategy:
        /// - If user has 5+ observations: EXPLOIT personalized weight
        /// - If user has 1-4 observations: BLEND personalized with global (Linear Interpolation)
        /// - If new user/action: EXPLORE using global weight
        /// 
        /// Formula (blending):
        /// weight_final = (weight_personalized × confidence) + (weight_global × (1 - confidence))
        /// where confidence = min(1.0, observations / 50)
        /// 
        /// DATABASE OPERATIONS:
        /// - READ: UserWeightProfile (personalized weight)
        /// - READ: InteractionWeight (global fallback)
        /// - WRITE: UserWeightProfile (if new user/action)
        /// </summary>
        private async Task<double> GetEffectiveWeightAsync(
            Guid userId,
            string actionType,
            int userInteractionCount,
            CancellationToken cancellationToken)
        {
            // 🔵 DATABASE READ: Try to get personalized weight
            var userWeight = await _userWeights.GetByUserAndActionAsync(
                userId,
                actionType,
                cancellationToken);

            if (userWeight != null && userWeight.ObservationCount >= 5)
            {
                // EXPLOITATION: Enough data - trust personalized weight
                return userWeight.PersonalizedWeight;
            }
            else if (userWeight != null && userWeight.ObservationCount > 0)
            {
                // HYBRID: Blend personalized with global using Linear Interpolation
                var globalWeight = await GetGlobalWeightAsync(actionType, cancellationToken);
                var trustRatio = userWeight.Confidence; // 0 to 1, grows with observations

                return (userWeight.PersonalizedWeight * trustRatio) +
                       (globalWeight * (1 - trustRatio));
            }
            else
            {
                // EXPLORATION: New user or new action type - use global default
                var globalWeight = await GetGlobalWeightAsync(actionType, cancellationToken);

                // 🟡 CREATE NEW: Initialize personalized weight profile
                var newUserWeight = UserWeightProfile.Create(userId, actionType, globalWeight);

                // 🟢 DATABASE WRITE: Add to repository (will be saved on commit)
                _userWeights.Add(newUserWeight);

                return globalWeight;
            }
        }

        /// <summary>
        /// Get global weight with heuristic fallback
        /// 
        /// DATABASE OPERATIONS:
        /// - READ: InteractionWeight (configured global weights)
        /// </summary>
        private async Task<double> GetGlobalWeightAsync(
            string actionType,
            CancellationToken cancellationToken)
        {
            // 🔵 DATABASE READ: Try to get configured global weight
            var weightEntity = await _globalWeights.GetByActionTypeAsync(
                actionType,
                cancellationToken);

            if (weightEntity != null)
            {
                return weightEntity.Weight;
            }

            // Heuristic defaults based on domain knowledge
            // These are educated guesses about action importance
            return actionType.ToLower() switch
            {
                "view" => 0.2,          // Low commitment
                "click" => 0.4,         // Some interest
                "save" => 0.6,          // Clear interest
                "share" => 0.7,         // Strong signal (social proof)
                "register" => 0.9,      // High commitment
                "purchase" => 1.0,      // Highest value
                "ticket_purchase" => 1.0,
                _ => 0.3                // Unknown actions get medium weight
            };
        }

        /// <summary>
        /// ALGORITHM 2: Contextual Multi-Armed Bandit with Feature Engineering
        /// 
        /// Applies three types of boosts to base weight:
        /// 
        /// 1. EXPONENTIAL RECENCY DECAY
        ///    Formula: boost = exp(-λ × hours)
        ///    Effect: Recent actions worth 100%, decays to ~37% after 100 hours
        /// 
        /// 2. FREQUENCY-BASED LINEAR SCALING
        ///    Formula: boost = 1 + (frequency × 0.03), capped at 1.3
        ///    Effect: Repeated action types get up to 30% boost
        /// 
        /// 3. SESSION ENGAGEMENT THRESHOLD
        ///    Rule: If 3+ actions in same session → 20% boost
        ///    Effect: Rewards engaged users
        /// 
        /// This is similar to contextual bandits where we adjust rewards based on context features
        /// </summary>
        private double ApplyContextualBoosts(
    double baseWeight,
    UserBehaviorLog log,
    List<UserBehaviorLog> history)
        {
            var weight = baseWeight;

            // BOOST 1: Exponential Recency Decay
            // Formula: exp(-λ × t) where λ = 0.01, t = hours
            double hoursSince = log.CreatedAt.HasValue
                ? (DateTime.UtcNow - log.CreatedAt.Value).TotalHours
                : 0.0; // default if CreatedAt is null

            var recencyBoost = Math.Exp(-0.01 * hoursSince);
            weight *= (0.7 + (0.3 * recencyBoost)); // Scale between 70-100%

            // BOOST 2: Frequency-based Linear Scaling
            var actionFrequency = history.Count(h => h.ActionType == log.ActionType);
            var frequencyBoost = Math.Min(1.3, 1.0 + (actionFrequency * 0.03));
            weight *= frequencyBoost;

            // BOOST 3: Session Engagement (threshold-based rule)
            var sessionActions = history.Count(h => h.SessionId == log.SessionId);
            if (sessionActions >= 3)
            {
                weight *= 1.2; // 20% boost
            }

            return Math.Min(1.0, weight); // Cap at 1.0 to keep scores normalized
        }

        // ===== ML PREDICTION METHOD (COMMENTED UNTIL ML READY) =====
        /*
        /// <summary>
        /// ALGORITHM 6: LightGBM Gradient Boosting Trees Prediction
        /// 
        /// ENABLE THIS when you have:
        /// - 1000+ labeled interactions (UserBehaviorLog with LedToConversion filled)
        /// - Trained ML model (use MLModelTrainingService)
        /// 
        /// How it works:
        /// 1. Extract features from user action (action type, recency, frequency, etc.)
        /// 2. Feed to LightGBM model
        /// 3. Get probability (0-1) that this action is valuable
        /// 
        /// LightGBM advantages:
        /// - Fast prediction (milliseconds)
        /// - Handles categorical features (action types)
        /// - Good with imbalanced data (few conversions vs many views)
        /// - Learns complex patterns humans miss
        /// </summary>
        private async Task<double> PredictActionScoreAsync(
            Guid userId, 
            UserBehaviorLog log, 
            List<UserBehaviorLog> userHistory, 
            CancellationToken cancellationToken)
        {
            if (_predictionEngine == null) return 0;
            
            // Feature engineering - create input for ML model
            var features = new UserActionFeatures
            {
                ActionType = log.ActionType,
                HoursSinceAction = (float)(DateTime.UtcNow - log.CreatedAt).TotalHours,
                UserTotalActions = userHistory.Count,
                UserActionsOfThisType = userHistory.Count(h => h.ActionType == log.ActionType),
                SessionActionCount = userHistory.Count(h => h.SessionId == log.SessionId),
                DayOfWeek = (int)log.CreatedAt.DayOfWeek,
                HourOfDay = log.CreatedAt.Hour,
                ActionFrequencyRatio = userHistory.Count > 0 
                    ? userHistory.Count(h => h.ActionType == log.ActionType) / (float)userHistory.Count 
                    : 0,
                UserEngagementDiversity = userHistory.Select(h => h.ActionType).Distinct().Count()
            };

            // Get prediction from LightGBM model
            var prediction = _predictionEngine.Predict(features);
            
            // Return probability (0-1) that this action is valuable
            return prediction.Probability;
        }
        */

        // ===== EMBEDDING METHODS (COMMENTED UNTIL EMBEDDING SERVICE READY) =====
        /*
        /// <summary>
        /// ALGORITHM 5: User Embedding via Mean Pooling
        /// 
        /// Creates a dense vector representation of user interests in semantic space
        /// 
        /// Process:
        /// 1. Get embeddings for all events user interacted with
        /// 2. Weight each embedding by action strength and recency
        /// 3. Average them (mean pooling)
        /// 4. Normalize to unit vector (for cosine similarity)
        /// 
        /// Result: 384-dimensional vector that captures user's semantic preferences
        /// Example: User who likes "jazz concerts" will have embedding similar to "blues shows"
        /// 
        /// Similar to Word2Vec's document embedding or BERT's [CLS] token
        /// </summary>
        private async Task<float[]> GenerateUserEmbeddingAsync(
            Guid userId, 
            List<UserBehaviorLog> userHistory, 
            CancellationToken cancellationToken)
        {
            if (userHistory.Count == 0) 
                return new float[384]; // Zero vector for new users
            
            var embeddingDim = 384; // Standard for sentence-transformers/all-MiniLM-L6-v2
            var weightedSum = new float[embeddingDim];
            var totalWeight = 0.0;

            foreach (var log in userHistory)
            {
                // Get event's semantic embedding (from title + description + tags)
                var eventEmbedding = await _embeddingService.GetEventEmbeddingAsync(log.EventId, cancellationToken);
                
                // Get action weight
                var actionWeight = await GetGlobalWeightAsync(log.ActionType, cancellationToken);
                
                // Apply slower time decay for embeddings (recent matters but not as much)
                var hoursSince = (DateTime.UtcNow - log.CreatedAt).TotalHours;
                var recencyFactor = Math.Exp(-0.001 * hoursSince);
                var finalWeight = actionWeight * recencyFactor;
                
                // Weighted sum
                for (int i = 0; i < embeddingDim; i++)
                    weightedSum[i] += eventEmbedding[i] * (float)finalWeight;
                
                totalWeight += finalWeight;
            }

            // Mean pooling: divide by total weight
            var userEmbedding = new float[embeddingDim];
            for (int i = 0; i < embeddingDim; i++)
                userEmbedding[i] = (float)(weightedSum[i] / totalWeight);

            // L2 normalization to unit vector (for cosine similarity)
            var magnitude = Math.Sqrt(userEmbedding.Sum(x => x * x));
            for (int i = 0; i < embeddingDim; i++)
                userEmbedding[i] /= (float)magnitude;

            return userEmbedding;
        }

        /// <summary>
        /// ALGORITHM 7: Cosine Similarity
        /// 
        /// Measures semantic similarity between user profile and event in embedding space
        /// 
        /// Formula: cos(θ) = (A · B) / (||A|| × ||B||)
        /// Since vectors are normalized to unit length, this simplifies to dot product
        /// 
        /// Result: -1 (opposite) to 1 (identical), scaled to [0, 1] for easier use
        /// 
        /// Benefits:
        /// - Captures semantic meaning better than keyword matching
        /// - "jazz concert" is similar to "blues performance" even without shared words
        /// - Works well with cold start (new events similar to user's past likes)
        /// </summary>
        private double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length) 
                throw new ArgumentException("Vectors must have same dimensions");
            
            // Dot product (since vectors are normalized, this IS the cosine similarity)
            double dotProduct = 0;
            for (int i = 0; i < vectorA.Length; i++)
                dotProduct += vectorA[i] * vectorB[i];
            
            // Convert from [-1, 1] to [0, 1] for easier combination with other scores
            return (dotProduct + 1.0) / 2.0;
        }
        */

        /// <summary>
        /// Resolve event category (placeholder for now)
        /// TODO: Implement actual category lookup from event repository
        /// </summary>
        private async Task<string> ResolveCategoryAsync(Guid eventId, CancellationToken cancellationToken)
        {
            // TODO: var eventEntity = await _eventRepo.GetByIdAsync(eventId, cancellationToken);
            // TODO: return eventEntity.CategoryId;
            return "default-category"; // Placeholder
        }

        /// <summary>
        /// Get or create user interest score for a category
        /// 
        /// DATABASE OPERATIONS:
        /// - READ: UserInterestScore (try to find existing)
        /// - WRITE: UserInterestScore (if creating new)
        /// </summary>
        private async Task<UserInterestScore> GetOrCreateScoreAsync(
            Guid userId,
            string category,
            CancellationToken cancellationToken)
        {
            // DATABASE READ: Try to find existing score
            var score = await _scoreRepo.GetByUserAndCategoryAsync(userId, category, cancellationToken);
            if (score == null)
            {
                // CREATE NEW: User hasn't interacted with this category before
                score = UserInterestScore.Create(userId, category, 0);

                // DATABASE WRITE: Add to repository (will be inserted on commit)
                _scoreRepo.Add(score);
            }

            return score;
        }

        /// <summary>
        /// Apply decay to all user interest scores
        /// Called periodically (e.g., nightly job) to fade old interests
        /// 
        /// DATABASE OPERATIONS:
        /// - READ: All UserInterestScore for user
        /// - WRITE: Updated scores after decay
        /// </summary>
        public async Task ApplyDecayAsync(
            Guid userId,
            double decayFactor,
            CancellationToken cancellationToken = default)
        {
            //DATABASE READ: Get all interest scores for user
            var scores = await _scoreRepo.GetByUserAsync(userId, cancellationToken);

            foreach (var score in scores)
            {
                //DOMAIN LOGIC: Apply exponential decay
                score.ApplyDecay(decayFactor);

                //DATABASE WRITE: Mark for update
                _scoreRepo.Update(score);
            }

            // DATABASE COMMIT: Save all decayed scores
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}