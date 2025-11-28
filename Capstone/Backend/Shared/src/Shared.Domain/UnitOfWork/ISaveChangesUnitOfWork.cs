using System.Threading;
using System.Threading.Tasks;

namespace Shared.Domain.UnitOfWork
{
    public interface ISaveChangesUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}


