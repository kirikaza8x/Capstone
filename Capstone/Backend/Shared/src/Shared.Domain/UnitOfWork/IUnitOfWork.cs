using Shared.Domain.Repositories;
using Shared.Domain.Common.DDD;

namespace Shared.Domain.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
    IRepository<T> Repository<T>() where T : class,IEntity<Guid>;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
} 