using System.Transactions;
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
            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                },
                TransactionScopeAsyncFlowOption.Enabled);

            var total = 0;
            foreach (var uow in _units)
            {
                total += await uow.SaveChangesAsync(cancellationToken);
            }

            scope.Complete();
            return total;
        }
    }
}