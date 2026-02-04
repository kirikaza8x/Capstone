using AI.Application.Abstractions;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;

namespace AI.Application.Services
{
    /// <summary>
    ///  ENTRY POINT for capturing user behavior
    /// 
    /// This service is called whenever user performs any action (view, click, purchase, etc.)
    /// 
    /// FLOW:
    /// 1. Create UserBehaviorLog entity
    /// 2. Save to database immediately (real-time tracking)
    /// 3. Trigger background aggregation to update interest scores
    /// 
    /// DATABASE OPERATIONS:
    /// - WRITE: UserBehaviorLog (immediate save)
    /// </summary>
    public class UserBehaviorTrackingService
    {
        private readonly IUserBehaviorLogRepository _logRepo;
        private readonly IAiUnitOfWork _unitOfWork;
        private readonly IProfileAggregatorService _aggregator;

        public UserBehaviorTrackingService(
            IUserBehaviorLogRepository logRepo,
            IAiUnitOfWork unitOfWork,
            IProfileAggregatorService aggregator)
        {
            _logRepo = logRepo;
            _unitOfWork = unitOfWork;
            _aggregator = aggregator;
        }

        /// <summary>
        /// CAPTURE user action and save to database
        /// 
        /// Called from:
        /// - API controllers when user interacts with events
        /// - Frontend tracking scripts
        /// - Mobile app analytics
        /// 
        /// DATABASE OPERATIONS:
        /// - WRITE: UserBehaviorLog (INSERT)
        /// </summary>
        public async Task TrackUserActionAsync(
            Guid sessionId,
            Guid userId,
            Guid eventId,
            string actionType,
            string? metadata = null,
            CancellationToken cancellationToken = default)
        {
            //CREATE DOMAIN ENTITY: Build behavior log
            var behaviorLog = UserBehaviorLog.Create(
                sessionId,
                userId,
                eventId,
                actionType,
                metadata
            );

            //DATABASE WRITE: Add log to repository
            _logRepo.Add(behaviorLog);

            // DATABASE COMMIT: Save immediately for real-time tracking
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //TRIGGER LEARNING: Process this action in background
            // Options for background processing:
            // - Task.Run (simple, in-process)
            // - Hangfire (persistent, retryable)
            // - Azure Functions (serverless)
            // - RabbitMQ/Kafka (distributed)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _aggregator.AggregateUserProfileAsync(userId, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error - in real system, use proper logging framework
                    Console.WriteLine($"Error aggregating profile for user {userId}: {ex.Message}");
                }
            }, cancellationToken);
        }
    }
}