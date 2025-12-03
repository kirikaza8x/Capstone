using Shared.Domain.UnitOfWork;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Infrastructure.Common
{
    public class CompositeUnitOfWork : ICompositeUnitOfWork
    {
        private readonly IEnumerable<IDbContextUnitOfWork> _units;

        public CompositeUnitOfWork(IEnumerable<IDbContextUnitOfWork> units)
        {
            _units = units;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Don't use TransactionScope with retry strategies - it conflicts with NpgsqlRetryingExecutionStrategy
            // The individual DbContexts will handle their own transactions with retry logic
            var total = 0;
            foreach (var uow in _units)
            {
                total += await uow.SaveChangesAsync(cancellationToken);
            }

            return total;
        }
    }
}