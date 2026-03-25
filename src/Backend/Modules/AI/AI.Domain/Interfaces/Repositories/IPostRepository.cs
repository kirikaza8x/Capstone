using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Shared.Domain.Data.Repositories;

namespace Marketing.Domain.Repositories;

/// <summary>
/// Domain-specific queries for the Post aggregate.
///
/// WHAT IS HERE:
///   - Queries with fixed business semantics used repeatedly across use cases
///   - Status-scoped lookups that encode domain rules (e.g. "pending queue")
///   - Tracking token lookup (unique constraint enforced at domain level)
/// </summary>
public interface IPostRepository : IRepository<PostMarketing, Guid>
{
    // ─────────────────────────────────────────────────────────────
    // Organizer Queries
    // Used by: organizer dashboard, manage posts list
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all posts created by an organizer for a specific event,
    /// ordered by CreatedAt descending.
    /// Covers: organizer "manage posts" screen per event.
    /// </summary>
    Task<IReadOnlyList<PostMarketing>> GetByEventAndOrganizerAsync(
        Guid eventId,
        Guid organizerId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all posts for an organizer across all their events,
    /// optionally filtered by status.
    /// Covers: organizer dashboard post overview.
    /// </summary>
    Task<IReadOnlyList<PostMarketing>> GetByOrganizerAsync(
        Guid organizerId,
        PostStatus? status = null,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Admin / Moderation Queries
    // Used by: admin pending review queue
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all posts currently awaiting admin review, ordered by
    /// SubmittedAt ascending (oldest first = FIFO review queue).
    /// Covers: admin moderation queue screen.
    /// </summary>
    Task<IReadOnlyList<PostMarketing>> GetPendingQueueAsync(
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Public / Attendee Queries
    // Used by: event detail page — show active marketing posts
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all published posts for an event, ordered by PublishedAt descending.
    /// Covers: attendee-facing event page showing active promotions.
    /// </summary>
    Task<IReadOnlyList<PostMarketing>> GetPublishedByEventAsync(
        Guid eventId,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Attribution / Tracking
    // Used by: UserBehaviorLog attribution when a post link is clicked
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks up a post by its tracking token.
    /// Used when a user clicks a post link → system records behavior log
    /// against the event, attributed to this post via the token.
    /// Token is unique per post (enforced at creation time).
    /// </summary>
    Task<PostMarketing?> GetByTrackingTokenAsync(
        string trackingToken,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Integrity Guard
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether a tracking token is already in use.
    /// Called during post creation to guarantee token uniqueness
    /// before persisting the aggregate.
    /// </summary>
    Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken ct = default);
}