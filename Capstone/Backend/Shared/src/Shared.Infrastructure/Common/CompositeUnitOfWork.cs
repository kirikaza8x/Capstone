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
            var total = 0;
            foreach (var uow in _units)
            {
                total += await uow.SaveChangesAsync(cancellationToken);
            }

            return total;
        }
    }
}