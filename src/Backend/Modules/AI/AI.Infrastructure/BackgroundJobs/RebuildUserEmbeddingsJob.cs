using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AI.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AI.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Quartz.NET job that periodically rebuilds stale user embeddings.
    /// Scheduled to run every 5 minutes by default.
    /// </summary>
    [DisallowConcurrentExecution]
    public class RebuildUserEmbeddingsJob : IJob
    {
        private readonly IUserEmbeddingRepository _embeddingRepo;
        private readonly IUserInterestScoreRepository _scoreRepo;
        private readonly IUserEmbeddingBuilder _builder;
        private readonly IAiUnitOfWork _uow;
        private readonly ILogger<RebuildUserEmbeddingsJob> _logger;

        public RebuildUserEmbeddingsJob(
            IUserEmbeddingRepository embeddingRepo,
            IUserInterestScoreRepository scoreRepo,
            IUserEmbeddingBuilder builder,
            IAiUnitOfWork uow,
            ILogger<RebuildUserEmbeddingsJob> logger)
        {
            _embeddingRepo = embeddingRepo;
            _scoreRepo = scoreRepo;
            _builder = builder;
            _uow = uow;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting scheduled user embedding rebuild job");

            try
            {
                var batchSize = context.MergedJobDataMap.ContainsKey("BatchSize")
                    ? context.MergedJobDataMap.GetInt("BatchSize")
                    : 100;

                var staleEmbeddings = await _embeddingRepo.GetStaleEmbeddingsAsync(context.CancellationToken);

                if (!staleEmbeddings.Any())
                {
                    _logger.LogInformation("No stale embeddings found");
                    return;
                }

                _logger.LogInformation("Found {Count} stale embeddings to rebuild", staleEmbeddings.Count);

                int succeeded = 0;
                int failed = 0;

                foreach (var embedding in staleEmbeddings.Take(batchSize))
                {
                    try
                    {
                        await RebuildUserEmbeddingAsync(embedding.UserId, context.CancellationToken);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to rebuild embedding for user {UserId}", embedding.UserId);
                        failed++;
                    }
                }

                _logger.LogInformation("Embedding rebuild complete: {Succeeded} succeeded, {Failed} failed",
                    succeeded, failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in user embedding rebuild job");
                throw;
            }
        }

        private async Task RebuildUserEmbeddingAsync(Guid userId, CancellationToken ct)
        {
            // Get user's interest scores
            var scores = await _scoreRepo.GetTopCategoriesAsync(userId, 50, ct);

            if (!scores.Any())
                return;

            // Build the user embedding vector
            var vector = _builder.Build(scores);
            var categories = scores.Select(x => x.Category).ToList();

            // Check if user already has an embedding
            var existingEmbedding = await _embeddingRepo.GetByUserIdAsync(userId, ct);

            if (existingEmbedding != null)
            {
                existingEmbedding.UpdateEmbedding(vector);
                foreach (var category in categories)
                {
                    existingEmbedding.AddContributingCategory(category);
                }
                _embeddingRepo.Update(existingEmbedding);
            }
            else
            {
                var embedding = UserEmbedding.Create(userId, vector, categories);
                _embeddingRepo.Add(embedding);
            }

            await _uow.SaveChangesAsync(ct);
        }
    }
}
