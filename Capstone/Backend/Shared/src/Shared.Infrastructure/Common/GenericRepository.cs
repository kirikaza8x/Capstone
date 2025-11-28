using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Shared.Domain.Repositories;
using Shared.Domain.Common.DDD;
using System.Linq.Expressions;

namespace Shared.Infrastructure.Common
{
    public class GenericRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbSet.AddAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error adding entity of type {typeof(T).Name}", ex);
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error adding range of entities of type {typeof(T).Name}", ex);
            }
        }

        public void Update(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating entity of type {typeof(T).Name}", ex);
            }
        }

        public virtual void UpdateRange(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbSet.AttachRange(entities);
                foreach (var entity in entities)
                {
                    _dbContext.Entry(entity).State = EntityState.Modified;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating range of entities of type {typeof(T).Name}", ex);
            }
        }

        public virtual async Task<int> UpdateRange(
                Expression<Func<T, bool>> predicate,
                Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
                CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(predicate)
                    .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error updating entities of type {typeof(T).Name} by predicate", ex);
            }
        }


        public void Delete(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var entry = _dbContext.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting entity of type {typeof(T).Name}", ex);
            }
        }

        public virtual void DeleteRange(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbSet.RemoveRange(entities);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting range of entities of type {typeof(T).Name}", ex);
            }
        }

        public virtual async Task<int> DeleteRange(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(predicate)
                    .ExecuteDeleteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error deleting entities of type {typeof(T).Name} by predicate", ex);
            }
        }


        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking existence of {typeof(T).Name}", ex);
            }
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet.AsNoTracking();
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                return await query.CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error counting entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            try
            {
                var query = GetQueryable();
                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }
                return await query.FirstOrDefaultAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving first or default {typeof(T).Name}", ex);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            try
            {
                var query = GetQueryable();
                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }
                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving all entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            try
            {
                var query = GetQueryable();
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }
                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error finding entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isAscending = true,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            try
            {
                var query = GetQueryable();
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (orderBy != null)
                {
                    query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                }

                var totalCount = await query.CountAsync(cancellationToken);
                var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving paged entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving entity of type {typeof(T).Name} by Id", ex);
            }
        }

        public IQueryable<T> GetQueryable(bool asNoTracking = true)
        {
            try
            {
                return asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting queryable for {typeof(T).Name}", ex);
            }
        }
    }
}
