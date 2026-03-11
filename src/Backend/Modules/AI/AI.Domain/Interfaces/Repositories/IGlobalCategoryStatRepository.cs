using AI.Domain.ReadModels;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    public interface IGlobalCategoryStatRepository : IRepository<GlobalCategoryStat, Guid>
    {

        Task<GlobalCategoryStat?> GetByCategoryAsync(string category);
        Task<List<GlobalCategoryStat>> GetByCategoryNamesAsync(List<string> categories);
        Task<List<GlobalCategoryStat>> GetTopCategoriesAsync(int topN = 20);
        Task<List<GlobalCategoryStat>> GetAllAsync();

        // Batch operations
        Task ApplyGlobalDecayAsync(double decayFactor);
        Task<int> GetTotalCategoriesAsync();
        Task<List<GlobalCategoryStat>> GetStaleStatsAsync(int daysThreshold = 90);
    }
}