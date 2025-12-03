using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Domain.Repositories;
using Shared.Domain.UnitOfWork;
using Shared.Domain.Common.DDD;
using System.Collections.Concurrent;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Infrastructure.Common
{
    public class GenericUnitOfWork<TDbContext> : IDbContextUnitOfWork, IUnitOfWork where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        public GenericUnitOfWork(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IRepository<T> Repository<T>() where T : class, IEntity<Guid>
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ =>
                new GenericRepository<T>(_dbContext));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Use execution strategy to allow retries
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(
                async () => await _dbContext.SaveChangesAsync(cancellationToken));
        }

        public bool IsInTransaction => _currentTransaction != null;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            // Don't use explicit transactions with retry strategy - let the execution strategy handle it
            // This is a no-op to maintain backward compatibility
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await RollbackTransactionAsync(cancellationToken);
                    throw;
                }
                finally
                {
                    await DisposeTransactionAsync();
                }
            }
            else
            {
                // No explicit transaction - just save changes with execution strategy
                var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(
                    async () => await _dbContext.SaveChangesAsync(cancellationToken));
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction to rollback.");

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _currentTransaction?.Dispose();
                _dbContext.Dispose();
            }

            _disposed = true;
        }
    }
}
