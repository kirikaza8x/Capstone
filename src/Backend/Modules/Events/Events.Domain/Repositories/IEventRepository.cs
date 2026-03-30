using Events.Domain.Entities;
using Events.Domain.Enums;
using Shared.Domain.Data.Repositories;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Domain.Repositories;

public interface IEventRepository : IRepository<Event, Guid>
{
    Task<Event?> GetByIdWithSessionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithTicketTypesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithTicketTypesAndAreasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithAreasAndSeatsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default);
    Task<Event?> GetByUrlPathAsync(string urlPath, CancellationToken cancellationToken = default);
    Task<bool> IsUrlPathExistsAsync(string urlPath, CancellationToken cancellationToken = default);

    Task<EventSession?> GetEventSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Event?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Event>> GetPublishedWithCategoriesAsync(
        PagedQuery pagedQuery,
        int? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<PagedResult<Event>> GetByOrganizerPagedAsync(Guid organizerId, EventStatus? status, PagedQuery pagedQuery, CancellationToken cancellationToken = default);

    Task<Event?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(Guid eventId, Guid userId, string permission, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetByCategoriesOrHashtagsAsync(
        IEnumerable<string> categoryNames,
        IEnumerable<string> hashtagNames,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetPublishedEndedEventsAsync(
        DateTime utcNow,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetEventsDueReminderAsync(
        DateTime utcNow,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetSuspendedExpiredEventsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetAllActivePagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Event?> GetByIdForReIndexAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TicketType>> GetTicketTypesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetMiniByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    Task<IReadOnlyCollection<Event>> GetAssignedEventsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Event>> SearchEventsAsync(
        string? keyword,
        PagedQuery query,
        CancellationToken cancellationToken = default);

}
