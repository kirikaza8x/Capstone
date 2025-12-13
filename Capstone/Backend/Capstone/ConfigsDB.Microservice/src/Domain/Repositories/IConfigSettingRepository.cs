using ConfigsDB.Domain.Entities;
using Shared.Domain.Repositories;

namespace ConfigsDB.Domain.Repositories
{
    // <summary>
    /// Specialized bulk repository contract for ConfigSetting.
    /// Inherits all generic bulk operations and allows adding domain-specific methods.
    /// </summary>
    public interface IConfigSettingBulkRepository : IBulkOperationRepository<ConfigSetting>
    {
        // Example of domain-specific bulk operation
        // Task BulkInsertWithEncryptionAsync(IEnumerable<ConfigSetting> entities, CancellationToken cancellationToken = default);

        // Task BulkDeactivateAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository interface for managing <see cref="ConfigSetting"/> aggregates.
    /// Extends the generic <see cref="IRepository{T}"/> with domain-specific queries
    /// for categorized configuration settings with environment overrides.
    /// </summary>
    public interface IConfigSettingRepository : IRepository<ConfigSetting> 
    {
        /// <summary>
        /// Retrieves a config setting by its unique identifier.
        /// </summary>
        Task<ConfigSetting?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a config setting by its key and environment.
        /// </summary>
        /// <param name="key">The configuration key (e.g., "ExpiryDays", "ConnectionString").</param>
        /// <param name="environment">Environment scope (e.g., "Global", "Dev", "Prod").</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ConfigSetting"/> or null if not found.</returns>
        Task<ConfigSetting?> GetByKeyAndEnvironmentAsync(string key, string environment, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all config settings with a specific key across all environments.
        /// Useful for finding all overrides of a particular configuration.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Collection of config settings with the same key in different environments.</returns>
        Task<IEnumerable<ConfigSetting>> GetByKeyAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all active config settings for a specific category.
        /// </summary>
        /// <param name="category">Category name (e.g., "JWT", "Database", "Email").</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>All active config settings in the specified category.</returns>
        Task<IEnumerable<ConfigSetting>> GetByCategoryAsync(string category, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all active config settings for a specific category and environment.
        /// </summary>
        /// <param name="category">Category name (e.g., "JWT", "Database").</param>
        /// <param name="environment">Environment scope (e.g., "Global", "Dev", "Prod").</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Active config settings matching both category and environment.</returns>
        Task<IEnumerable<ConfigSetting>> GetByCategoryAndEnvironmentAsync(
            string category, 
            string environment, 
            CancellationToken ct = default);

        /// <summary>
        /// Retrieves all active config settings for a specific environment.
        /// </summary>
        /// <param name="environment">Environment scope (e.g., "Global", "Dev", "Prod").</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>All active config settings for the environment.</returns>
        Task<IEnumerable<ConfigSetting>> GetByEnvironmentAsync(string environment, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all categories with their config counts.
        /// Useful for admin dashboards and configuration overview.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Dictionary of category names to their active config count.</returns>
        Task<Dictionary<string, int>> GetCategoriesWithCountsAsync(CancellationToken ct = default);

        /// <summary>
        /// Checks if a config setting exists with the given key and environment.
        /// </summary>
        /// <param name="key">Configuration key.</param>
        /// <param name="environment">Environment scope.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string key, string environment, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all encrypted config settings (for security auditing).
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>All config settings marked as encrypted.</returns>
        Task<IEnumerable<ConfigSetting>> GetEncryptedConfigsAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves all inactive/deactivated config settings.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>All inactive config settings.</returns>
        Task<IEnumerable<ConfigSetting>> GetInactiveConfigsAsync(CancellationToken ct = default);

        /// <summary>
        /// Searches config settings by key pattern (case-insensitive).
        /// Useful for admin search functionality.
        /// </summary>
        /// <param name="keyPattern">Search pattern (e.g., "jwt", "connection").</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Config settings matching the search pattern.</returns>
        Task<IEnumerable<ConfigSetting>> SearchByKeyAsync(string keyPattern, CancellationToken ct = default);

        /// <summary>
        /// Gets config settings modified after a specific date.
        /// Useful for tracking configuration changes over time.
        /// </summary>
        /// <param name="afterDate">Date threshold.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Config settings modified after the specified date.</returns>
        Task<IEnumerable<ConfigSetting>> GetModifiedAfterAsync(DateTime afterDate, CancellationToken ct = default);

        /// <summary>
        /// Gets config settings created by a specific user.
        /// Useful for audit trails and user activity tracking.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Config settings created by the user.</returns>
        Task<IEnumerable<ConfigSetting>> GetByCreatorAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all existing key/environment pairs from the database for a given set of keys.
        /// Useful for batch duplicate detection in bulk create scenarios.
        /// </summary>
        /// <param name="keys">Distinct key/environment pairs to check.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Set of key/environment pairs that already exist.</returns>
        Task<HashSet<(string Key, string Environment)>> GetExistingKeysAsync(
            IEnumerable<(string Key, string Environment)> keys,
            CancellationToken ct = default);
    }
}