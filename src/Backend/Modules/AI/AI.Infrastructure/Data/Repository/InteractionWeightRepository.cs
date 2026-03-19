using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using AI.Domain.Entities;
using AI.Domain.Repositories;
using AI.Infrastructure.Data;

namespace AI.Infrastructure.Repositories;

public class InteractionWeightRepository : RepositoryBase<InteractionWeight, Guid>, IInteractionWeightRepository
{
    private readonly AIModuleDbContext _dbContext;
    private readonly DbSet<InteractionWeight> _dbSet;

    public InteractionWeightRepository(AIModuleDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<InteractionWeight>();
    }

    public async Task<InteractionWeight?> GetByActionTypeAsync(
        string actionType,
        string version = "default",
        CancellationToken ct = default)
    {
        var normalized = actionType.ToLowerInvariant().Trim();
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ActionType == normalized &&
                x.Version == version &&
                x.IsActive,
            ct);
    }

    public async Task<List<InteractionWeight>> GetActiveWeightsAsync(
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);
    }

    public async Task<List<InteractionWeight>> GetAllVersionsForActionAsync(
        string actionType,
        CancellationToken ct = default)
    {
        var normalized = actionType.ToLowerInvariant().Trim();
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.ActionType == normalized)
            .ToListAsync(ct);
    }

    public async Task ActivateVersionAsync(
        string actionType,
        string versionToActivate,
        CancellationToken ct = default)
    {
        var normalized = actionType.ToLowerInvariant().Trim();

        var allVersions = await _dbSet
            .Where(x => x.ActionType == normalized)
            .ToListAsync(ct);

        foreach (var weight in allVersions)
        {
            if (weight.Version == versionToActivate)
                weight.Reactivate();
            else
                weight.Deactivate();
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<InteractionWeight> UpsertAsync(
        string actionType,
        double weight,
        string? description = null,
        string version = "default",
        CancellationToken ct = default)
    {
        var normalized = actionType.ToLowerInvariant().Trim();

        var existing = await _dbSet
            .FirstOrDefaultAsync(x =>
                x.ActionType == normalized &&
                x.Version == version,
            ct);

        if (existing != null)
        {
            existing.UpdateWeight(weight, description);
            _dbSet.Update(existing);
        }
        else
        {
            var newWeight = InteractionWeight.Create(normalized, weight, description, version);
            await _dbSet.AddAsync(newWeight, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
        return existing ?? await _dbSet.FirstAsync(x =>
            x.ActionType == normalized && x.Version == version, ct);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var weight = await _dbSet.FindAsync(new object[] { id }, ct);
        if (weight != null)
        {
            weight.Deactivate();
            _dbSet.Update(weight);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        return false;
    }

    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(x => x.IsActive, ct);
    }

    public async Task<Dictionary<string, double>> GetAllActiveWeightsAsDictionaryAsync(
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToDictionaryAsync(x => x.ActionType, x => x.Weight, ct);
    }
}