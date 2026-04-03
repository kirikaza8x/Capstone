using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Shared.Domain.Data.Repositories;

namespace Marketing.Domain.Repositories;

public interface IPostRepository : IRepository<PostMarketing, Guid>
{
    // Organizer
    Task<IReadOnlyList<PostMarketing>> GetByEventAndOrganizerAsync(
        Guid eventId,
        Guid organizerId,
        CancellationToken ct = default);

    Task<IReadOnlyList<PostMarketing>> GetByOrganizerAsync(
        Guid organizerId,
        PostStatus? status = null,
        CancellationToken ct = default);

    // Admin
    Task<IReadOnlyList<PostMarketing>> GetPendingQueueAsync(
        CancellationToken ct = default);

    // Public
    Task<IReadOnlyList<PostMarketing>> GetPublishedByEventAsync(
        Guid eventId,
        CancellationToken ct = default);

    ///  NEW: Global feed (homepage / recommendation fallback)
    Task<IReadOnlyList<PostMarketing>> GetGlobalFeedAsync(
        int limit,
        CancellationToken ct = default);

    ///  NEW: Slug lookup (public URL)
    Task<PostMarketing?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default);

    // Tracking
    Task<PostMarketing?> GetByTrackingTokenAsync(
        string trackingToken,
        CancellationToken ct = default);

    Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken ct = default);
    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken ct = default);

    Task<IReadOnlyList<PostMarketing>> GetPublishedNotDistributedToAsync(
        ExternalPlatform platform,
        int limit = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a specific distribution record for a post + platform.
    /// Returns null if not found.
    /// Used for checking status or updating distribution details.
    /// </summary>
    Task<ExternalDistribution?> GetDistributionByPostAndPlatformAsync(
        Guid postId,
        ExternalPlatform platform,
        CancellationToken ct = default);
}