using Events.Domain.Entities;
using Events.Domain.Enums;
using Shared.Domain.Data;

namespace Events.Domain.Repositories;

public interface IEventRepository : IRepository<Event, Guid>
{
    Task<Event?> GetByIdWithSessionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithAreasAndSeatsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default);
    Task<Event?> GetByUrlPathAsync(string urlPath, CancellationToken cancellationToken = default);
    Task<bool> IsUrlPathExistsAsync(string urlPath, CancellationToken cancellationToken = default);
}