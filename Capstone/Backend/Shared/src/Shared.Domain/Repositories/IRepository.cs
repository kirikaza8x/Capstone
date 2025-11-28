using System.Linq.Expressions;
using Shared.Domain.Common.DDD;

namespace Shared.Domain.Repositories
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        void Update(T entity, CancellationToken cancellationToken = default);
        void UpdateRange(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        void Delete(T entity , CancellationToken cancellationToken = default);
        void DeleteRange(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default);
        IQueryable<T> GetQueryable(bool asNoTracking = true);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, bool isAscending = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    }
}