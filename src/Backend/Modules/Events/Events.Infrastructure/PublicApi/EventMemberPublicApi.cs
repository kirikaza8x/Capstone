using Events.Domain.Repositories;
using Events.Infrastructure.Caching;
using Events.PublicApi.Constants;
using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Microsoft.Extensions.Caching.Distributed;

namespace Events.Infrastructure.PublicApi;

internal class EventMemberPublicApi(
    IEventRepository   eventRepository,
    IDistributedCache  distributedCache)
    : IEventMemberPublicApi
{
    private static readonly TimeSpan PermissionCacheTtl = TimeSpan.FromSeconds(60);

    public async Task<bool> HasPermissionAsync(
        Guid              eventId,
        Guid              userId,
        string            permission,
        CancellationToken cancellationToken = default)
    {
        if (!EventMemberPermission.All.Contains(permission))
            return false;

        var cacheKey    = EventPermissionCacheKeys.Permission(eventId, userId, permission);
        var cachedValue = await distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (cachedValue is not null)
            return cachedValue == bool.TrueString;

        var hasPermission = await eventRepository
            .HasPermissionAsync(eventId, userId, permission, cancellationToken);

        await distributedCache.SetStringAsync(
            cacheKey,
            hasPermission.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PermissionCacheTtl
            },
            cancellationToken);

        return hasPermission;
    }

    public async Task<IReadOnlyList<EventRecommendationFeature>> GetEventsByCategoriesOrHashtagsAsync(
        IEnumerable<string> categoryNames,
        IEnumerable<string> hashtagNames,
        CancellationToken   cancellationToken = default)
    {
        var events = await eventRepository
            .GetByCategoriesOrHashtagsAsync(categoryNames, hashtagNames, cancellationToken);

        return MapToFeatures(events);
    }

    public async Task<IReadOnlyList<EventRecommendationFeature>> GetMiniByIdsAsync(
        IEnumerable<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        var ids    = eventIds.ToList();
        if (ids.Count == 0) return Array.Empty<EventRecommendationFeature>();

        var events = await eventRepository
            .GetMiniByIdsAsync(ids, cancellationToken);

        return MapToFeatures(events);
    }

    public async Task<EventRecommendationFeature?> GetByIdForReIndexAsync(
        Guid              eventId,
        CancellationToken cancellationToken = default)
    {
        var evt = await eventRepository
            .GetByIdForReIndexAsync(eventId, cancellationToken);

        return evt is null ? null : MapToFeatures(new[] { evt }).FirstOrDefault();
    }

    public async Task<IReadOnlyList<EventRecommendationFeature>> GetAllForReIndexAsync(
        int               page              = 1,
        int               pageSize          = 100,
        CancellationToken cancellationToken = default)
    {
        var events = await eventRepository
            .GetAllActivePagedAsync(page, pageSize, cancellationToken);

        return MapToFeatures(events);
    }

    // ── Private ───────────────────────────────────────────────────

    private static IReadOnlyList<EventRecommendationFeature> MapToFeatures(
        IEnumerable<Events.Domain.Entities.Event> events)
    {
        return events.Select(e => new EventRecommendationFeature
        {
            Id           = e.Id,
            Title        = e.Title,
            Location     = e.Location,
            BannerUrl    = e.BannerUrl,
            EventStartAt = e.EventStartAt,
            EventEndAt   = e.EventEndAt,
            MinPrice     = e.TicketTypes.Min(t => (decimal?)t.Price),
            MaxPrice     = e.TicketTypes.Max(t => (decimal?)t.Price),
            Categories   = e.EventCategories.Select(ec => ec.Category.Name).ToList(),
            Hashtags     = e.EventHashtags.Select(eh => eh.Hashtag.Name).ToList()
        }).ToList();
    }
}
