using System.Linq.Expressions;
using Shared.Domain.DDD;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Shared.Domain.Data.Repositories;

public partial interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> SearchAsync(
        Expression<Func<TEntity, string>> field,
        string keyword,
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> GetAllWithPagingAsync(
        PagedQuery pagedQuery,
        Expression<Func<TEntity, bool>>? predicate = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> GetPagedAsync(
        AdvancedPagedQuery query,
        Expression<Func<TEntity, bool>>? predicate = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TResult>> GetPagedAsync<TResult>(
        AdvancedPagedQuery query,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default);
}
