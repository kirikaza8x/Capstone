using ConfigsDB.Domain.Entities;
using ConfigsDB.Domain.Repositories;
using ConfigsDB.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Common;

namespace ConfigsDB.Infrastructure.Repositories
{
    public class ConfigSettingRepository : GenericRepository<ConfigSetting>, IConfigSettingRepository
    {
        private readonly ConfigSettingDbContext _dbContext;
        private readonly DbSet<ConfigSetting> _dbSet;

        public ConfigSettingRepository(ConfigSettingDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.ConfigSettings;
        }

        public async Task<ConfigSetting?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<ConfigSetting?> GetByKeyAndEnvironmentAsync(string key, string environment, CancellationToken ct = default) =>
            await _dbSet.FirstOrDefaultAsync(x =>
                x.Key.ToLower() == key.ToLower() &&
                x.Environment.ToLower() == environment.ToLower() &&
                x.IsActive,
                ct);

        public async Task<IEnumerable<ConfigSetting>> GetByKeyAsync(string key, CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.Key.ToLower() == key.ToLower() && x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> GetByCategoryAsync(string category, CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.Category.ToLower() == category.ToLower() && x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> GetByCategoryAndEnvironmentAsync(
            string category,
            string environment,
            CancellationToken ct = default) =>
            await _dbSet
                .Where(x =>
                    x.Category.ToLower() == category.ToLower() &&
                    x.Environment.ToLower() == environment.ToLower() &&
                    x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> GetByEnvironmentAsync(string environment, CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.Environment.ToLower() == environment.ToLower() && x.IsActive)
                .ToListAsync(ct);

        public async Task<Dictionary<string, int>> GetCategoriesWithCountsAsync(CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.IsActive)
                .GroupBy(x => x.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count, ct);

        public async Task<bool> ExistsAsync(string key, string environment, CancellationToken ct = default) =>
            await _dbSet.AnyAsync(x =>
                x.Key.ToLower() == key.ToLower() &&
                x.Environment.ToLower() == environment.ToLower(),
                ct);

        public async Task<IEnumerable<ConfigSetting>> GetEncryptedConfigsAsync(CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.IsEncrypted && x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> GetInactiveConfigsAsync(CancellationToken ct = default) =>
            await _dbSet
                .Where(x => !x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> SearchByKeyAsync(string keyPattern, CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.Key.ToLower().Contains(keyPattern.ToLower()) && x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> GetModifiedAfterAsync(DateTime afterDate, CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.ModifiedAt > afterDate && x.IsActive)
                .ToListAsync(ct);

        public async Task<IEnumerable<ConfigSetting>> GetByCreatorAsync(string userId, CancellationToken ct = default) =>
            await _dbSet
                .Where(x => x.CreatedBy == userId)
                .ToListAsync(ct);
        public async Task<HashSet<(string Key, string Environment)>> GetExistingKeysAsync(
            IEnumerable<(string Key, string Environment)> keys,
            CancellationToken ct = default)
        {
            var keyList = keys.ToList();

            var existing = await _dbContext.Set<ConfigSetting>()
                .Where(cs => keyList.Any(k => k.Key == cs.Key && k.Environment == cs.Environment))
                .Select(cs => new { cs.Key, cs.Environment })
                .ToListAsync(ct);
            return existing
                .Select(x => (x.Key, x.Environment))
                .ToHashSet();
        }

    }
}
