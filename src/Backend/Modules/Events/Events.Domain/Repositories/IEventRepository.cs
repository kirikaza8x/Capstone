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

    Task<EventSession?> GetEventSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Event?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Event> Events, int TotalCount)> GetPagedAsync(
        string? searchTerm = null,
        EventStatus? status = null,
        int? categoryId = null,
        Guid? organizerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool isDescending = true,
        CancellationToken cancellationToken = default);
}