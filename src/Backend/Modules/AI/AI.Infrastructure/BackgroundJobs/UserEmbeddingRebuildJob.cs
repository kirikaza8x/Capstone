using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AI.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Background job that rebuilds user embeddings when they become stale.
    /// Triggered by:
    /// - New UserBehaviorLog entries (via domain events)
    /// - Scheduled Quartz.NET job for batch rebuilds
    /// </summary>
    public class UserEmbeddingRebuildJob
    {
        private readonly IUserEmbeddingRepository _embeddingRepo;
        private readonly IUserInterestScoreRepository _scoreRepo;
        private readonly IUserEmbeddingBuilder _builder;
        private readonly IAiUnitOfWork _uow;
        private readonly ILogger<UserEmbeddingRebuildJob> _logger;

        public UserEmbeddingRebuildJob(
            IUserEmbeddingRepository embeddingRepo,
            IUserInterestScoreRepository scoreRepo,
            IUserEmbeddingBuilder builder,
            IAiUnitOfWork uow,
            ILogger<UserEmbeddingRebuildJob> logger)
        {
            _embeddingRepo = embeddingRepo;
            _scoreRepo = scoreRepo;
            _builder = builder;
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Rebuilds embedding for a single user.
        /// Called when new behavior logs arrive for that user.
        /// </summary>
        public async Task RebuildUserEmbeddingAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Rebuilding embedding for user {UserId}", userId);

            // Get user's interest scores (weighted by recency and action type)
            var scores = await _scoreRepo.GetTopCategoriesAsync(userId, 50, ct);

            if (!scores.Any())
            {
                _logger.LogWarning("No interest scores found for user {UserId}", userId);
                return;
            }

            // Build the user embedding vector
            var vector = _builder.Build(scores);

            // Get the list of contributing categories
            var categories = scores.Select(x => x.Category).ToList();

            // Check if user already has an embedding
            var existingEmbedding = await _embeddingRepo.GetByUserIdAsync(userId, ct);

            if (existingEmbedding != null)
            {
                // Update existing embedding
                existingEmbedding.UpdateEmbedding(vector);
                foreach (var category in categories)
                {
                    existingEmbedding.AddContributingCategory(category);
                }
                _embeddingRepo.Update(existingEmbedding);
            }
            else
            {
                // Create new embedding
                var embedding = UserEmbedding.Create(userId, vector, categories);
                _embeddingRepo.Add(embedding);
            }

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Embedding rebuilt for user {UserId} with {Count} categories",
                userId, categories.Count);
        }

        /// <summary>
        /// Batch rebuilds all stale user embeddings.
        /// Called by scheduled Quartz.NET job.
        /// </summary>
        public async Task RebuildAllStaleAsync(
            int batchSize = 100,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Starting batch rebuild of stale user embeddings");

            var staleEmbeddings = await _embeddingRepo.GetStaleEmbeddingsAsync(ct);

            if (!staleEmbeddings.Any())
            {
                _logger.LogInformation("No stale embeddings found");
                return;
            }

            _logger.LogInformation("Found {Count} stale embeddings to rebuild", staleEmbeddings.Count);

            int processed = 0;
            int succeeded = 0;
            int failed = 0;

            foreach (var embedding in staleEmbeddings.Take(batchSize))
            {
                try
                {
                    await RebuildUserEmbeddingAsync(embedding.UserId, ct);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild embedding for user {UserId}", embedding.UserId);
                    failed++;
                }

                processed++;

                if (processed % 10 == 0)
                {
                    _logger.LogInformation("Progress: {Processed}/{Total} processed, {Succeeded} succeeded, {Failed} failed",
                        processed, Math.Min(batchSize, staleEmbeddings.Count), succeeded, failed);
                }
            }

            _logger.LogInformation("Batch rebuild complete: {Processed} processed, {Succeeded} succeeded, {Failed} failed",
                processed, succeeded, failed);
        }

        /// <summary>
        /// Marks a user's embedding as stale, triggering a rebuild on next job run.
        /// Called when new UserBehaviorLog entries are created.
        /// </summary>
        public async Task MarkUserEmbeddingStaleAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            var embedding = await _embeddingRepo.GetByUserIdAsync(userId, ct);

            if (embedding != null)
            {
                embedding.MarkStale();
                _embeddingRepo.Update(embedding);
                await _uow.SaveChangesAsync(ct);
                _logger.LogDebug("Marked embedding as stale for user {UserId}", userId);
            }
        }
    }
}
