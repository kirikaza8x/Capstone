using Events.PublicApi.Records;

namespace Events.PublicApi.PublicApi;

public interface IEventMemberPublicApi
{
    Task<bool> HasPermissionAsync(
        Guid eventId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventRecommendationFeature>> GetEventsByCategoriesOrHashtagsAsync(
        IEnumerable<string> categoryNames,
        IEnumerable<string> hashtagNames,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single event by ID for re-indexing.</summary>
    Task<EventRecommendationFeature?> GetByIdForReIndexAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active events for re-indexing into Qdrant.
    /// Paginated to avoid loading thousands of events into memory at once.
    /// </summary>
    Task<IReadOnlyList<EventRecommendationFeature>> GetAllForReIndexAsync(
        int page = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default);
}