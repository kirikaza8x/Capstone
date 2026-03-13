using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore; // Required for distance extension methods

namespace AI.Infrastructure.Repositories
{
    public class CategoryEmbeddingRepository : RepositoryBase<CategoryEmbedding, Guid>, ICategoryEmbeddingRepository
    {
        private readonly AIModuleDbContext _dbContext;
        private readonly DbSet<CategoryEmbedding> _dbSet;
        private readonly IUserEmbeddingRepository _userEmbeddingRepository;

        public CategoryEmbeddingRepository(
            AIModuleDbContext dbContext,
            IUserEmbeddingRepository userEmbeddingRepository)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<CategoryEmbedding>();
            _userEmbeddingRepository = userEmbeddingRepository;
        }

        // ===== SINGLE LOOKUPS =====

        public async Task<CategoryEmbedding?> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            var normalised = category.ToLowerInvariant().Trim();

            return await _dbSet
                .FirstOrDefaultAsync(x => x.Category == normalised, cancellationToken);
        }

        public async Task<List<CategoryEmbedding>> GetByCategoriesAsync(
            IEnumerable<string> categories,
            CancellationToken cancellationToken = default)
        {
            var normalised = categories.Select(c => c.ToLowerInvariant().Trim()).ToList();

            if (!normalised.Any())
                return new List<CategoryEmbedding>();

            return await _dbSet
                .AsNoTracking()
                .Where(x => normalised.Contains(x.Category))
                .ToListAsync(cancellationToken);
        }

        // ===== SIMILARITY SEARCH =====

        public async Task<List<CategoryEmbedding>> FindSimilarCategoriesAsync(
            float[] embedding,
            int topN = 10,
            double minSimilarity = 0.0,
            CancellationToken cancellationToken = default)
        {
            var queryVector = new Vector(embedding);


            var results = await _dbSet
                .AsNoTracking()
                .Where(ce => ce.IsActive)
                .OrderBy(ce => ((Vector)(object)ce.Embedding!).CosineDistance(queryVector))
                .Take(topN)
                .ToListAsync(cancellationToken);

            return results;
        }

        // public async Task<List<CategoryEmbedding>> RecommendCategoriesForUserAsync(
        //     Guid userId,
        //     int topN = 20,
        //     CancellationToken cancellationToken = default)
        // {
        //     var userEmbedding = await _userEmbeddingRepository.GetByUserIdAsync(userId, cancellationToken);

        //     if (userEmbedding == null || userEmbedding.Embedding == null)
        //     {
        //         return await _dbSet
        //             .AsNoTracking()
        //             .Where(ce => ce.IsActive && ce.RecommendationCount > 0)
        //             .OrderByDescending(ce => ce.CTR)
        //             .ThenByDescending(ce => ce.RecommendationCount)
        //             .Take(topN)
        //             .ToListAsync(cancellationToken);
        //     }

        //     var queryVector = new Vector(userEmbedding.Embedding);

        //     return await _dbSet
        //         .AsNoTracking()
        //         .Where(ce => ce.IsActive)
        //         .OrderBy(ce => ((Vector)(object)ce.Embedding!).CosineDistance(queryVector))
        //         .Take(topN)
        //         .ToListAsync(cancellationToken);
        // }

        // ===== SEARCH & DISCOVERY =====

        public async Task<List<CategoryEmbedding>> SearchByDescriptionAsync(
            string keyword,
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            var lowerKeyword = keyword.ToLowerInvariant();

            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Description.ToLower().Contains(lowerKeyword))
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryEmbedding>> GetAllCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // ===== ANALYTICS =====

        public async Task<List<CategoryEmbedding>> GetLowCTRAsync(
            double threshold = 0.1,
            int minRecommendations = 10,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.RecommendationCount >= minRecommendations && x.CTR < threshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryEmbedding>> GetHighCTRAsync(
            double threshold = 0.3,
            int minRecommendations = 10,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.RecommendationCount >= minRecommendations && x.CTR >= threshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryEmbedding>> GetNotUpdatedSinceAsync(
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.LastUpdated < since)
                .ToListAsync(cancellationToken);
        }

        // ===== MODEL MANAGEMENT =====

        public async Task<List<CategoryEmbedding>> GetByModelAsync(
            string modelName,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.ModelName == modelName)
                .ToListAsync(cancellationToken);
        }

        // ===== CLEANUP =====

        public async Task<List<CategoryEmbedding>> GetUnusedAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.RecommendationCount == 0 && x.ClickThroughCount == 0)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CategoryEmbedding>> GetPopularAsync(
    int topN,
    CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ce => ce.IsActive && ce.RecommendationCount > 0)
                .OrderByDescending(ce => ce.CTR)
                .ThenByDescending(ce => ce.RecommendationCount)
                .Take(topN)
                .ToListAsync(cancellationToken);
        }
    }
}