using System.Text.Json;
using Events.Domain.Enums;
using Events.Domain.Repositories;
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
    private static readonly TimeSpan PermissionCacheTtl = TimeSpan.FromHours(24);

    public async Task<bool> HasPermissionAsync(
            Guid eventId,
            Guid userId,
            string requiredPermission,
            CancellationToken cancellationToken = default)
    {
        var cacheKey = $"event_permissions:{eventId}:{userId}";

        // read cache
        var cachedJson = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedPermissions = JsonSerializer.Deserialize<List<string>>(cachedJson);
            return HasRequiredPermission(cachedPermissions, requiredPermission);
        }

        // cache Miss -> query db
        var @event = await eventRepository.GetByIdWithMembersAsync(eventId, cancellationToken);

        if (@event is null)
        {
            return false;
        }

        // get list permissions
        List<string> permissionsToCache = [];

        if (@event.OrganizerId == userId)
        {
            permissionsToCache.Add(EventPermissions.Organizer);
        }
        else
        {
            var member = @event.Members.FirstOrDefault(m => m.UserId == userId && m.Status == EventMemberStatus.Active);
            if (member is not null)
            {
                permissionsToCache.AddRange(member.Permissions);
            }
        }

        // cache permissions if any
        if (permissionsToCache.Count > 0)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PermissionCacheTtl
            };

            var json = JsonSerializer.Serialize(permissionsToCache);
            await distributedCache.SetStringAsync(cacheKey, json, options, cancellationToken);
        }

        return HasRequiredPermission(permissionsToCache, requiredPermission);
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
    private static bool HasRequiredPermission(IReadOnlyCollection<string>? userPermissions, string requiredPermission)
    {
        if (userPermissions is null || userPermissions.Count == 0)
            return false;

        return userPermissions.Contains(EventPermissions.Organizer) ||
               userPermissions.Contains(requiredPermission);
    }

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
