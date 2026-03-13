using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data.Seeds;
using AI.Domain.Entities;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Persistence.Seeds;

public class InteractionWeightSeeder(AIModuleDbContext context) : IDataSeeder<InteractionWeight>
{
    public async Task SeedAllAsync()
    {
        var existingWeights = await context.Set<InteractionWeight>()
            .Select(w => w.ActionType)
            .ToListAsync();

        var weightsToAdd = new List<InteractionWeight>();

        // // Ticket-related
        // if (!existingWeights.Contains("ticket_purchase"))
        //     weightsToAdd.Add(InteractionWeight.Create("ticket_purchase", 30.0, "User purchased a ticket"));

        // if (!existingWeights.Contains("ticket_view"))
        //     weightsToAdd.Add(InteractionWeight.Create("ticket_view", 2.0, "User viewed ticket details"));

        // if (!existingWeights.Contains("ticket_cancel"))
        //     weightsToAdd.Add(InteractionWeight.Create("ticket_cancel", 10.0, "User canceled a ticket"));

        // General platform interactions
        if (!existingWeights.Contains("view"))
            weightsToAdd.Add(InteractionWeight.Create("view", 1.0, "Basic view action"));

        if (!existingWeights.Contains("click"))
            weightsToAdd.Add(InteractionWeight.Create("click", 5.0, "User clicked on item"));

        if (!existingWeights.Contains("like"))
            weightsToAdd.Add(InteractionWeight.Create("like", 3.0, "User liked content"));

        if (!existingWeights.Contains("share"))
            weightsToAdd.Add(InteractionWeight.Create("share", 7.0, "User shared content"));

        if (!existingWeights.Contains("comment"))
            weightsToAdd.Add(InteractionWeight.Create("comment", 6.0, "User commented on content"));

        if (!existingWeights.Contains("wishlist_add"))
            weightsToAdd.Add(InteractionWeight.Create("wishlist_add", 8.0, "User added item to wishlist"));

        if (!existingWeights.Contains("purchase"))
            weightsToAdd.Add(InteractionWeight.Create("purchase", 25.0, "User purchased item"));

        if (weightsToAdd.Count > 0)
        {
            await context.Set<InteractionWeight>().AddRangeAsync(weightsToAdd);
        }

        await context.SaveChangesAsync();
    }
}
