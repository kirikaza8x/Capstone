using Shared.Domain.Common.DDD;
using Shared.Domain.Common.Options;
namespace Shared.Domain.Repositories
{
    /// <summary>
    /// Specialized repository interface for high-performance data operations.
    /// This contract resides in Infrastructure but exposes only provider-agnostic options.
    /// </summary>
    /// <typeparam name="T">Entity type implementing <see cref="IEntity"/>.</typeparam>
    public interface IBulkOperationRepository<T> where T : class, IEntity
    {
        // -------------------------------------------------------------------
        #region Bulk Operations (using BulkInsertOptions abstraction)

        /// <summary>
        /// Bulk insert entities efficiently.
        /// </summary>
        Task BulkInsertAsync(IEnumerable<T> entities, BulkInsertOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk update entities efficiently.
        /// </summary>
        Task BulkUpdateAsync(IEnumerable<T> entities, BulkInsertOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk delete entities efficiently.
        /// </summary>
        Task BulkDeleteAsync(IEnumerable<T> entities, BulkInsertOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk insert or update (upsert) entities efficiently.
        /// </summary>
        Task BulkInsertOrUpdateAsync(IEnumerable<T> entities, BulkInsertOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk read entities from database (useful for synchronization scenarios).
        /// </summary>
        Task<List<T>> BulkReadAsync(IEnumerable<T> entities, BulkInsertOptions? options = null, CancellationToken cancellationToken = default);

        #endregion
    }
}
