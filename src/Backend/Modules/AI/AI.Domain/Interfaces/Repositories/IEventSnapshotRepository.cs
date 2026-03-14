using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    public interface IEventSnapshotRepository : IRepository<EventSnapshot, Guid>
    {
        Task<EventSnapshot?> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
        Task<List<Guid>> GetUnembeddedEventIdsAsync(
            int batchSize, CancellationToken ct = default);
    }
}