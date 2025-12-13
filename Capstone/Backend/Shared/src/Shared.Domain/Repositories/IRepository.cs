using System.Linq.Expressions;
using Shared.Domain.Common.DDD;

namespace Shared.Domain.Repositories
{
    /// <summary>
    /// Generic repository interface for aggregate roots and entities.
    /// Defines common data access operations while keeping persistence concerns
    /// separate from domain logic. Saving changes is handled by the Unit of Work.
    /// </summary>
    /// <typeparam name="T">Entity type implementing <see cref="IEntity"/>.</typeparam>
    public interface IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Adds a single entity to the repository.
        /// </summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a collection of entities to the repository.
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a single entity in the repository.
        /// </summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a collection of entities in the repository.
        /// </summary>
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a single entity from the repository.
        /// Returns true if the entity was successfully marked for deletion.
        /// </summary>
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a collection of entities from the repository.
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the given predicate.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts the number of entities matching the given predicate.
        /// If no predicate is provided, counts all entities.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the first entity matching the given predicate, or null if none found.
        /// Supports eager loading via includes.
        /// </summary>
        Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Retrieves all entities in the repository.
        /// Supports eager loading via includes.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Retrieves an entity by its primary key.
        /// </summary>
        Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Returns a queryable for advanced scenarios.
        ///// </summary>
        //IQueryable<T> GetQueryable(bool asNoTracking = true);

        /// <summary>
        /// Finds entities matching the given predicate.
        /// Supports eager loading via includes.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Retrieves a paged set of entities matching the given predicate.
        /// Supports ordering and eager loading.
        /// Returns both the items and the total count.
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isAscending = true,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);
    }
}
