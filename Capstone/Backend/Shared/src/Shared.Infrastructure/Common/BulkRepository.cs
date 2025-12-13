using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common.DDD;
using Shared.Domain.Common.Options;
using Shared.Domain.Repositories;

namespace Shared.Infrastructure.Common
{
    public class BulkRepository<T> : IBulkOperationRepository<T> where T : class, IEntity
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public BulkRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        #region Bulk Operations (EFCore.BulkExtensions / IBulkOperationRepository<T>)

        public async Task BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                await _dbContext.BulkInsertAsync(entityList, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk inserting entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task BulkInsertAsync(IEnumerable<T> entities, BulkInsertOptions? options, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                var config = MapOptions(options);
                await _dbContext.BulkInsertAsync(entityList, config, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk inserting entities of type {typeof(T).Name} with options", ex);
            }
        }

        public async Task BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                await _dbContext.BulkUpdateAsync(entityList, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk updating entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task BulkUpdateAsync(IEnumerable<T> entities, BulkInsertOptions? options, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                var config = MapOptions(options);
                await _dbContext.BulkUpdateAsync(entityList, config, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk updating entities of type {typeof(T).Name} with options", ex);
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                await _dbContext.BulkDeleteAsync(entityList, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk deleting entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<T> entities, BulkInsertOptions? options, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                var config = MapOptions(options);
                await _dbContext.BulkDeleteAsync(entityList, config, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk deleting entities of type {typeof(T).Name} with options", ex);
            }
        }

        public async Task BulkInsertOrUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                await _dbContext.BulkInsertOrUpdateAsync(entityList, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk inserting or updating entities of type {typeof(T).Name}", ex);
            }
        }

        public async Task BulkInsertOrUpdateAsync(IEnumerable<T> entities, BulkInsertOptions? options, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return;

                var config = MapOptions(options);
                await _dbContext.BulkInsertOrUpdateAsync(entityList, config, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk inserting or updating entities of type {typeof(T).Name} with options", ex);
            }
        }

        public async Task<List<T>> BulkReadAsync(IEnumerable<T> entities, BulkInsertOptions? options = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var entityList = entities.ToList();
                if (!entityList.Any()) return new List<T>();

                var config = MapOptions(options);
                await _dbContext.BulkReadAsync(entityList, config, cancellationToken: cancellationToken);
                return entityList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error bulk reading entities of type {typeof(T).Name}", ex);
            }
        }

        #endregion

        private BulkConfig MapOptions(BulkInsertOptions? options)
        {
            return new BulkConfig
            {
                BatchSize = options?.BatchSize ?? 2000,
                PreserveInsertOrder = options?.PreserveInsertOrder ?? false,
                SetOutputIdentity = options?.SetOutputIdentity ?? false,
                UseTempDB = options?.UseTempDB ?? false
            };
        }
    }
}
