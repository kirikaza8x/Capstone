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
}
