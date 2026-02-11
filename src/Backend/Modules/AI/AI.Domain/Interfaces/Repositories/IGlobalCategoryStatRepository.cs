using AI.Domain.ReadModels;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    public interface IGlobalCategoryStatRepository : IRepository<GlobalCategoryStat, Guid>
    {
      
        /// <summary>
        /// RECOMMENDATION SERVICE USES THIS:
        /// "Give me the stats for all categories so I can calculate Bayesian averages"
        /// </summary>
        Task<List<GlobalCategoryStat>> GetAllAsync();

        // BACKGROUND JOB USES THIS:
        // "Find the stat for 'Jazz' so I can update it"
        Task<GlobalCategoryStat?> GetByCategoryAsync(string category);

        Task<List<GlobalCategoryStat>> GetTopCategoriesAsync(int count=10);

        Task<List<GlobalCategoryStat>> GetByCategoryNamesAsync(List<string> categoryNames);
        
    }
}