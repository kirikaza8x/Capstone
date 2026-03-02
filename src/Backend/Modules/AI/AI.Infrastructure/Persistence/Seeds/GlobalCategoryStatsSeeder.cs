using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data.Seeds;
using AI.Domain.ReadModels;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Persistence.Seeds;

public class GlobalCategoryStatsSeeder(AIModuleDbContext context) : IDataSeeder<GlobalCategoryStat>
{
    public async Task SeedAllAsync()
    {
        // ===== Seed GlobalCategoryStats =====
        var existingCategories = await context.Set<GlobalCategoryStat>()
            .Select(c => c.Category)
            .ToListAsync();

        var categoriesToAdd = new List<GlobalCategoryStat>();

        // Core categories
        if (!existingCategories.Contains("ticket"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("ticket", 50, 1000, rawScore: 1200));

        if (!existingCategories.Contains("concert"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("concert", 70, 1500));

        if (!existingCategories.Contains("sports_event"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("sports_event", 65, 1200));

        if (!existingCategories.Contains("conference"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("conference", 55, 900));

        if (!existingCategories.Contains("festival"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("festival", 60, 1100));

        // Enrichment categories
        if (!existingCategories.Contains("theater"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("theater", 58, 950));

        if (!existingCategories.Contains("opera"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("opera", 45, 600));

        if (!existingCategories.Contains("exhibition"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("exhibition", 40, 500));

        if (!existingCategories.Contains("workshop"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("workshop", 35, 400));

        if (!existingCategories.Contains("seminar"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("seminar", 30, 350));

        if (!existingCategories.Contains("standup_comedy"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("standup_comedy", 50, 700));

        if (!existingCategories.Contains("movie_premiere"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("movie_premiere", 55, 800));

        if (!existingCategories.Contains("charity_event"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("charity_event", 25, 300));

        if (!existingCategories.Contains("networking"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("networking", 28, 320));

        if (!existingCategories.Contains("cultural_event"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("cultural_event", 42, 550));

        if (!existingCategories.Contains("food_and_drink"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("food_and_drink", 48, 650));

        if (!existingCategories.Contains("family_event"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("family_event", 52, 720));

        if (!existingCategories.Contains("education"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("education", 33, 400));

        if (!existingCategories.Contains("religious_event"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("religious_event", 20, 250));

        if (!existingCategories.Contains("virtual_event"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("virtual_event", 38, 500));

        if (!existingCategories.Contains("exclusive_vip"))
            categoriesToAdd.Add(GlobalCategoryStat.Create("exclusive_vip", 60, 850));

        if (categoriesToAdd.Count > 0)
        {
            await context.Set<GlobalCategoryStat>().AddRangeAsync(categoriesToAdd);
        }

        await context.SaveChangesAsync();
    }
}
