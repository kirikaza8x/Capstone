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
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public bool IsInTransaction => _currentTransaction != null;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction to commit.");

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
