using AI.Domain.Entities;
using AI.Infrastructure.Data;
using AI.PublicApi.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data.Seeds;

namespace AI.Infrastructure.Persistence.Seeds;

/// <summary>
/// Seeds InteractionWeight for every action type defined in ActionTypes.
/// Weights are aligned with the semantic meaning of each action:
///   - Passive (view)               → low weight
///   - Engagement (click/like/etc.) → medium weight
///   - Conversion (purchase/etc.)   → high weight
///
/// Re-running is safe — only inserts missing action types.
/// To change a weight: update directly in DB or add an UpdateWeight() call below.
/// </summary>
public class InteractionWeightSeeder(AIModuleDbContext context) : IDataSeeder<InteractionWeight>
{
    public async Task SeedAllAsync()
    {
        var existing = await context.Set<InteractionWeight>()
            .Select(w => w.ActionType)
            .ToListAsync();

        var definitions = new[]
        {
            // ── Passive ───────────────────────────────────────────
            (ActionTypes.View,      1.0,  "User viewed content"),

            // ── Engagement ────────────────────────────────────────
            (ActionTypes.Click,     3.0,  "User clicked on item"),
            (ActionTypes.Like,      4.0,  "User liked content"),
            (ActionTypes.Bookmark,  6.0,  "User bookmarked content"),
            (ActionTypes.Comment,   6.0,  "User commented on content"),
            (ActionTypes.Share,     8.0,  "User shared content"),

            // ── Conversion ────────────────────────────────────────
            (ActionTypes.Checkout,  15.0, "User started checkout"),
            (ActionTypes.Subscribe, 20.0, "User subscribed"),
            (ActionTypes.Signup,    20.0, "User signed up"),
            (ActionTypes.Purchase,  25.0, "User completed purchase"),
        };

        var toAdd = definitions
            .Where(d => !existing.Contains(d.Item1))
            .Select(d => InteractionWeight.Create(d.Item1, d.Item2, d.Item3))
            .ToList();

        if (toAdd.Count > 0)
        {
            await context.Set<InteractionWeight>().AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
        }
    }
}
