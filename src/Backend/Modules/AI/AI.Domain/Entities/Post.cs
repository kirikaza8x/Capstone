using Marketing.Domain.Enums;
using Marketing.Domain.Events;
using Marketing.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;

namespace Marketing.Domain.Entities;

/// <summary>
/// Represents an AI-generated marketing post for an Event on the AIPromo platform.
///
/// LIFECYCLE:
///   Draft → Pending → Approved → Published → Archived
///                  ↘ Rejected  → (Organizer edits) → Pending (resubmit)
///
/// RESPONSIBILITY:
///   - Owns the content and its moderation state.
///   - Stores a TrackingToken used by UserBehaviorLog to attribute event interactions
///     back to this post (post itself does NOT own analytics — the event does).
///   - External distribution (Facebook, etc.) is handled downstream by n8n; this
///     entity only tracks whether that handoff succeeded.
///
/// LOOSE COUPLING:
///   - Stores EventId (Guid) only. Event details fetched via Events.PublicApi.
///   - Stores OrganizerId (Guid) only. User details fetched via Users.PublicApi.
/// </summary>
public sealed class PostMarketing : AggregateRoot<Guid>
{
    // =========================================================
    // Identity & Ownership
    // =========================================================

    /// <summary>The event this post is promoting.</summary>
    public Guid EventId { get; private set; }

    /// <summary>The organizer who created this post.</summary>
    public Guid OrganizerId { get; private set; }

    // =========================================================
    // Content
    // =========================================================

    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    // =========================================================
    // AI Metadata
    // =========================================================

    /// <summary>The prompt the organizer submitted to the AI.</summary>
    public string? PromptUsed { get; private set; }
    public string? AiModel { get; private set; }
    public int? AiTokensUsed { get; private set; }

    // =========================================================
    // Moderation
    // =========================================================

    public PostStatus Status { get; private set; }

    /// <summary>Admin who reviewed this post. Null until reviewed.</summary>
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // =========================================================
    // Publishing
    // =========================================================

    public DateTime? PublishedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>
    /// Opaque token embedded in post links.
    /// When a user clicks the post → navigates to event,
    /// UserBehaviorLog records: targetId=EventId, metadata={post_token}.
    /// This is how we attribute event traffic back to the post without
    /// Post owning any analytics counters itself.
    /// </summary>
    public string TrackingToken { get; private set; } = string.Empty;

    /// <summary>
    /// Set once n8n (or any downstream automation) confirms
    /// the post was distributed to an external platform.
    /// Null = AIPromo platform only.
    /// </summary>
    public string? ExternalPostUrl { get; private set; }

    // =========================================================
    // Versioning (supports edit-after-rejection pattern)
    // =========================================================

    public int Version { get; private set; }

    // =========================================================
    // EF Core
    // =========================================================

    private PostMarketing() { }

    // =========================================================
    // Factory
    // =========================================================

    /// <summary>
    /// Create a new AI-generated post in Draft status.
    /// Called after the AI service returns generated content.
    /// </summary>
    public static PostMarketing CreateDraft(
        Guid eventId,
        Guid organizerId,
        string title,
        string body,
        string trackingToken,
        string? imageUrl = null,
        string? promptUsed = null,
        string? aiModel = null,
        int? aiTokensUsed = null)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId is required.", nameof(eventId));
        if (organizerId == Guid.Empty)
            throw new ArgumentException("OrganizerId is required.", nameof(organizerId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));
        if (string.IsNullOrWhiteSpace(trackingToken))
            throw new ArgumentException("TrackingToken is required.", nameof(trackingToken));

        var post = new PostMarketing
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            OrganizerId = organizerId,
            Title = title.Trim(),
            Body = body.Trim(),
            ImageUrl = imageUrl?.Trim(),
            PromptUsed = promptUsed?.Trim(),
            AiModel = aiModel?.Trim(),
            AiTokensUsed = aiTokensUsed,
            TrackingToken = trackingToken.Trim().ToLowerInvariant(),
            Status = PostStatus.Draft,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
        };

        post.RaiseDomainEvent(new PostCreatedDomainEvent(
            PostId: post.Id,
            @eventId: post.EventId,
            OrganizerId: post.OrganizerId,
            CreatedAt: post.CreatedAt!.Value));

        return post;
    }

    // =========================================================
    // Organizer Behaviors
    // =========================================================

    /// <summary>
    /// Organizer edits draft content before submitting.
    /// Also allowed after rejection (reopen → edit → resubmit).
    /// </summary>
    public Result Update(
        string? title,
        string? body,
        string? imageUrl)
    {
        if (Status is not (PostStatus.Draft or PostStatus.Rejected))
            return Result.Failure(MarketingErrors.Post.CannotEditInStatus(Status));

        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(MarketingErrors.Post.TitleCannotBeEmpty);
            Title = title.Trim();
        }

        if (body is not null)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Result.Failure(MarketingErrors.Post.BodyCannotBeEmpty);
            Body = body.Trim();
        }

        ImageUrl = imageUrl?.Trim() ?? ImageUrl;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Organizer submits the draft for admin review.
    /// Can also resubmit after rejection.
    /// </summary>
    public Result Submit()
    {
        if (Status is not (PostStatus.Draft or PostStatus.Rejected))
            return Result.Failure(MarketingErrors.Post.CannotSubmitInStatus(Status));

        // Guard: must have content before submitting
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Body))
            return Result.Failure(MarketingErrors.Post.ContentIncompleteForSubmit);

        Status = PostStatus.Pending;
        RejectionReason = null; // clear previous rejection when resubmitting
        ModifiedAt = DateTime.UtcNow;
        SubmittedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostSubmittedDomainEvent(
            PostId: Id,
            @eventId: EventId,
            OrganizerId: OrganizerId,
            SubmittedAt: ModifiedAt!.Value));

        return Result.Success();
    }

    /// <summary>
    /// Organizer publishes the post to the AIPromo platform.
    /// Only allowed after admin approval.
    /// </summary>
    public Result Publish()
    {
        if (Status != PostStatus.Approved)
            return Result.Failure(MarketingErrors.Post.CannotPublishInStatus(Status));

        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostPublishedDomainEvent(
            PostId: Id,
            @eventId: EventId,
            OrganizerId: OrganizerId,
            PublishedAt: PublishedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Organizer soft-deletes the post (draft/rejected only).
    /// Published posts must be archived, not deleted.
    /// </summary>
    public Result Archive()
    {
        if (Status == PostStatus.Archived)
            return Result.Success(); // idempotent

        if (Status == PostStatus.Pending)
            return Result.Failure(MarketingErrors.Post.CannotArchiveWhilePending);

        Status = PostStatus.Archived;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostArchivedDomainEvent(Id, EventId, OrganizerId));

        return Result.Success();
    }

    // =========================================================
    // Admin Behaviors
    // =========================================================

    /// <summary>
    /// Admin approves the post — organizer can now publish it.
    /// </summary>
    public Result Approve(Guid adminId)
    {
        if (Status != PostStatus.Pending)
            return Result.Failure(MarketingErrors.Post.CannotApproveInStatus(Status));

        if (adminId == Guid.Empty)
            return Result.Failure(MarketingErrors.Post.ReviewerRequired);

        Status = PostStatus.Approved;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = null;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostApprovedDomainEvent(
            PostId: Id,
            @eventId: EventId,
            OrganizerId: OrganizerId,
            AdminId: adminId,
            ApprovedAt: ReviewedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Admin rejects the post with a mandatory reason.
    /// Organizer can edit and resubmit after rejection.
    /// </summary>
    public Result Reject(Guid adminId, string reason)
    {
        if (Status != PostStatus.Pending)
            return Result.Failure(MarketingErrors.Post.CannotRejectInStatus(Status));

        if (adminId == Guid.Empty)
            return Result.Failure(MarketingErrors.Post.ReviewerRequired);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(MarketingErrors.Post.RejectionReasonRequired);

        Status = PostStatus.Rejected;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason.Trim();
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostRejectedDomainEvent(
            PostId: Id,
            @eventId: EventId,
            OrganizerId: OrganizerId,
            AdminId: adminId,
            Reason: RejectionReason,
            RejectedAt: ReviewedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Admin force-removes a published post that violates platform policy.
    /// Different from Archive (which is organizer-initiated).
    /// </summary>
    public Result ForceRemove(Guid adminId, string reason)
    {
        if (Status == PostStatus.Archived)
            return Result.Success(); // idempotent

        if (adminId == Guid.Empty)
            return Result.Failure(MarketingErrors.Post.ReviewerRequired);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(MarketingErrors.Post.RejectionReasonRequired);

        Status = PostStatus.Archived;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason.Trim();
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostForceRemovedDomainEvent(
            PostId: Id,
            @eventId: EventId,
            AdminId: adminId,
            Reason: RejectionReason,
            RemovedAt: ReviewedAt.Value));

        return Result.Success();
    }

    // =========================================================
    // External Distribution (n8n callback)
    // =========================================================

    /// <summary>
    /// Called when n8n confirms the post was distributed to an external platform.
    /// This is informational only — the post lifecycle is already complete at Published.
    /// </summary>
    public Result RecordExternalDistribution(string externalUrl)
    {
        if (Status != PostStatus.Published)
            return Result.Failure(MarketingErrors.Post.CannotRecordDistributionInStatus(Status));

        if (string.IsNullOrWhiteSpace(externalUrl))
            return Result.Failure(MarketingErrors.Post.ExternalUrlRequired);

        ExternalPostUrl = externalUrl.Trim();
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // =========================================================
    // Queries
    // =========================================================

    public bool IsVisible() => Status == PostStatus.Published;
    public bool IsEditable() => Status is PostStatus.Draft or PostStatus.Rejected;
    public bool IsDistributedExternally() => !string.IsNullOrWhiteSpace(ExternalPostUrl);

    // =========================================================
    // Event sourcing stub (not used — state managed directly)
    // =========================================================

    protected override void Apply(IDomainEvent @event) { }
}