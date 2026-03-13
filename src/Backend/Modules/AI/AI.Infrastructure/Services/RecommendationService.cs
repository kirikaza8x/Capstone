// using AI.Application.Services;
// using AI.Domain.Entities;
// using AI.Domain.Repositories;

// namespace AI.Infrastructure.Services;

// public class RecommendationService
// {
//     private readonly IUserEmbeddingRepository _userEmbeddings;
//     private readonly ICategoryEmbeddingRepository _categoryEmbeddings;

//     public RecommendationService(
//         IUserEmbeddingRepository userEmbeddings,
//         ICategoryEmbeddingRepository categoryEmbeddings)
//     {
//         _userEmbeddings = userEmbeddings;
//         _categoryEmbeddings = categoryEmbeddings;
//     }

//     public async Task<List<CategoryEmbedding>> RecommendCategories(
//         Guid userId,
//         int topN,
//         CancellationToken ct)
//     {
//         var userEmbedding = await _userEmbeddings.GetByUserIdAsync(userId, ct);

//         if (userEmbedding == null || userEmbedding.Embedding == null)
//         {
//             return await _categoryEmbeddings.GetPopularAsync(topN, ct);
//         }

//         return await _categoryEmbeddings.FindSimilarCategoriesAsync(
//             userEmbedding.Embedding,
//             topN,
//             0.0,
//             ct);
//     }
// }