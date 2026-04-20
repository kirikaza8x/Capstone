// PostMarketing.cs
using Marketing.Domain.Enums;
using Marketing.Domain.Events;
using Marketing.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;

namespace Marketing.Domain.Entities;

/// <summary>
/// Aggregate root representing a marketing post for an event.
/// Manages content, moderation workflow, and external distribution.
/// </summary>
public sealed class PostMarketing : AggregateRoot<Guid>
{
    // =========================================================
    // Identity & Ownership
    // =========================================================

    public Guid EventId { get; private set; }
    public Guid OrganizerId { get; private set; }

    // =========================================================
    // Content
    // =========================================================

    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    // =========================================================
    // AI Metadata
    // =========================================================

    public string? PromptUsed { get; private set; }
    public string? AiModel { get; private set; }
    public int? AiTokensUsed { get; private set; }
    public decimal? AiCost { get; private set; }

    // =========================================================
    // Moderation & Lifecycle
    // =========================================================

    public PostStatus Status { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public string TrackingToken { get; private set; } = string.Empty;

    // =========================================================
    // External Distributions
    // =========================================================

    private readonly List<ExternalDistribution> _externalDistributions = new();

    public IReadOnlyList<ExternalDistribution> ExternalDistributions
        => _externalDistributions.AsReadOnly();

    // =========================================================
    // Versioning & EF Core
    // =========================================================

    public int Version { get; private set; }

    private PostMarketing() { }

    // =========================================================
    // Factory Method
    // =========================================================

    public static PostMarketing CreateDraft(
        Guid eventId,
        Guid organizerId,
        string title,
        string body,
        string trackingToken,
        string? summary = null,
        string? slug = null,
        string? imageUrl = null,
        string? promptUsed = null,
        string? aiModel = null,
        int? aiTokensUsed = null,
        decimal? aiCost = null)
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
            Summary = summary?.Trim(),
            Slug = slug ?? GenerateSlug(title),
            ImageUrl = imageUrl?.Trim(),
            PromptUsed = promptUsed?.Trim(),
            AiModel = aiModel?.Trim(),
            AiTokensUsed = aiTokensUsed,
            AiCost = aiCost,
            TrackingToken = trackingToken.Trim().ToLowerInvariant(),
            Status = PostStatus.Draft,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
        };

        post.RaiseDomainEvent(new PostCreatedDomainEvent(
            PostId: post.Id,
            TargetId: post.EventId,
            OrganizerId: post.OrganizerId,
            CreatedAt: post.CreatedAt!.Value));

        return post;
    }

    // =========================================================
    // Organizer Behaviors
    // =========================================================

    public Result Update(
        string? title,
        string? body,
        string? summary,
        string? imageUrl,
        string? slug = null,
        string? promptUsed = null,
        string? aiModel = null,
        int? additionalTokensUsed = null,
        decimal? additionalAiCost = null,
        string? trackingToken = null)
    {
        if (Status is not (PostStatus.Draft or PostStatus.Rejected))
            return Result.Failure(MarketingErrors.Post.CannotEditInStatus(Status));

        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(MarketingErrors.Post.TitleCannotBeEmpty);

            Title = title.Trim();
            Slug = slug ?? GenerateSlug(Title);
        }

        if (body is not null)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Result.Failure(MarketingErrors.Post.BodyCannotBeEmpty);

            Body = body.Trim();
        }

        if (summary is not null) Summary = summary.Trim();
        if (slug is not null) Slug = slug.Trim().ToLowerInvariant();
        if (imageUrl is not null) ImageUrl = imageUrl.Trim();
        if (promptUsed is not null) PromptUsed = promptUsed.Trim();
        if (aiModel is not null) AiModel = aiModel.Trim();
        if (additionalTokensUsed.HasValue) AiTokensUsed = (AiTokensUsed ?? 0) + additionalTokensUsed.Value;
        if (additionalAiCost.HasValue) AiCost = (AiCost ?? 0) + additionalAiCost.Value;
        if (trackingToken is not null) TrackingToken = trackingToken.Trim().ToLowerInvariant();

        Version++;
        ModifiedAt = DateTime.UtcNow;

        if (Status == PostStatus.Approved || Status == PostStatus.Published)
            Status = PostStatus.Draft;

        return Result.Success();
    }

    public Result Submit()
    {
        if (Status is not (PostStatus.Draft or PostStatus.Rejected))
            return Result.Failure(MarketingErrors.Post.CannotSubmitInStatus(Status));

        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Body))
            return Result.Failure(MarketingErrors.Post.ContentIncompleteForSubmit);

        SubmittedAt = DateTime.UtcNow;
        ModifiedAt = SubmittedAt;
        Status = PostStatus.Pending;
        RejectionReason = null;

        RaiseDomainEvent(new PostSubmittedDomainEvent(
            PostId: Id,
            TargetId: EventId,
            OrganizerId: OrganizerId,
            SubmittedAt: SubmittedAt.Value));

        return Result.Success();
    }

    public Result Publish()
    {
        if (Status != PostStatus.Approved)
            return Result.Failure(MarketingErrors.Post.CannotPublishInStatus(Status));

        if (PublishedAt is not null)
            return Result.Failure(MarketingErrors.Post.PublishFailed("Post is already published."));

        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        ModifiedAt = PublishedAt;

        RaiseDomainEvent(new PostPublishedDomainEvent(
            PostId: Id,
            TargetId: EventId,
            OrganizerId: OrganizerId,
            PublishedAt: PublishedAt.Value));

        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == PostStatus.Archived)
            return Result.Success();

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
        ModifiedAt = ReviewedAt;

        RaiseDomainEvent(new PostApprovedDomainEvent(
            PostId: Id,
            TargetId: EventId,
            OrganizerId: OrganizerId,
            AdminId: adminId,
            ApprovedAt: ReviewedAt.Value));

        return Result.Success();
    }

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
        ModifiedAt = ReviewedAt;

        RaiseDomainEvent(new PostRejectedDomainEvent(
            PostId: Id,
            TargetId: EventId,
            OrganizerId: OrganizerId,
            AdminId: adminId,
            Reason: RejectionReason,
            RejectedAt: ReviewedAt.Value));

        return Result.Success();
    }

    public Result AdminRemove(Guid adminId, string reason)
    {
        if (Status == PostStatus.Archived)
            return Result.Success();

        if (adminId == Guid.Empty)
            return Result.Failure(MarketingErrors.Post.ReviewerRequired);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(MarketingErrors.Post.RejectionReasonRequired);

        Status = PostStatus.Archived;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason.Trim();
        ModifiedAt = ReviewedAt;

        RaiseDomainEvent(new PostForceRemovedDomainEvent(
            PostId: Id,
            TargetId: EventId,
            AdminId: adminId,
            Reason: RejectionReason,
            RemovedAt: ReviewedAt.Value));

        return Result.Success();
    }

    // =========================================================
    // External Distribution Behaviors
    // =========================================================

    /// <summary>
    /// Queues this published post for distribution to an external platform.
    /// </summary>
    public Result QueueForExternalDistribution(ExternalPlatform platform, bool allowRetry = false)
    {
        if (Status != PostStatus.Published)
            return Result.Failure(MarketingErrors.Post.CannotDistributeInStatus(Status));

        if (platform == ExternalPlatform.Unknown)
            return Result.Failure(MarketingErrors.Distribution.PlatformRequired);

        if (!allowRetry)
        {
            var existingPending = _externalDistributions.FirstOrDefault(d =>
                d.Platform == platform && !d.IsSent() && !d.IsFailed());

            if (existingPending is not null)
                return Result.Failure(MarketingErrors.Distribution.AlreadyQueued(platform));
        }
        else
        {
            var oldDistribution = _externalDistributions.FirstOrDefault(d =>
                d.Platform == platform && !d.IsSent());

            if (oldDistribution is not null)
                _externalDistributions.Remove(oldDistribution);
        }

        var distribution = ExternalDistribution.Create(
            postMarketingId: Id,
            platform: platform);

        _externalDistributions.Add(distribution);

        ModifiedAt = DateTime.UtcNow;
        Version++;

        RaiseDomainEvent(new PostQueuedForDistributionDomainEvent(
            PostId: Id,
            Platform: platform,
            QueuedAt: DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Confirms successful distribution to external platform.
    /// Called by n8n callback after Facebook/LinkedIn post succeeds.
    /// </summary>
    public Result ConfirmExternalDistribution(
        ExternalPlatform platform,
        string externalUrl,
        string? externalPostId = null,
        string? platformMetadata = null)
    {
        var distribution = _externalDistributions.FirstOrDefault(d =>
            d.Platform == platform && d.IsPending());

        if (distribution is null)
            return Result.Failure(MarketingErrors.Distribution.NotFound(platform));

        if (string.IsNullOrWhiteSpace(externalUrl))
            return Result.Failure(MarketingErrors.Distribution.UrlRequired);

        distribution.UpdateExternalUrl(externalUrl);
        distribution.UpdateMetadata(externalPostId, platformMetadata);
        distribution.MarkAsSent();

        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostDistributedToPlatformDomainEvent(
            PostId: Id,
            Platform: platform,
            ExternalUrl: externalUrl,
            DistributedAt: distribution.SentAt!.Value));

        return Result.Success();
    }

    /// <summary>
    /// Records a failed distribution attempt.
    /// </summary>
    public Result FailExternalDistribution(ExternalPlatform platform, string errorMessage)
    {
        var distribution = _externalDistributions.FirstOrDefault(d =>
            d.Platform == platform && d.IsPending());

        if (distribution is null)
            return Result.Failure(MarketingErrors.Distribution.NotFound(platform));

        distribution.MarkAsFailed(errorMessage);
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Removes a distribution record for a platform.
    /// </summary>
    public Result RemoveExternalDistribution(ExternalPlatform platform)
    {
        var distribution = _externalDistributions.FirstOrDefault(d => d.Platform == platform);

        if (distribution is null)
            return Result.Failure(MarketingErrors.Distribution.NotFound(platform));

        _externalDistributions.Remove(distribution);
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // =========================================================
    // Query Methods
    // =========================================================

    public bool IsVisible() => Status == PostStatus.Published;
    public bool IsEditable() => Status is PostStatus.Draft or PostStatus.Rejected;
    public bool IsDistributedTo(ExternalPlatform platform)
        => _externalDistributions.Any(d => d.Platform == platform && d.IsSent());
    public ExternalDistribution? GetDistribution(ExternalPlatform platform)
        => _externalDistributions.FirstOrDefault(d => d.Platform == platform);
    public IReadOnlyList<ExternalDistribution> GetPendingDistributions()
        => _externalDistributions.Where(d => d.IsPending()).ToList().AsReadOnly();
    public IReadOnlyList<ExternalDistribution> GetSuccessfulDistributions()
        => _externalDistributions.Where(d => d.IsSent()).ToList().AsReadOnly();
    public IReadOnlyList<ExternalDistribution> GetFailedDistributions()
        => _externalDistributions.Where(d => d.IsFailed()).ToList().AsReadOnly();

    // =========================================================
    // Helpers
    // =========================================================

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        return title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("?", "")
            .Replace("!", "")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(":", "")
            .Replace(";", "");
    }

    protected override void Apply(IDomainEvent @event) { }
}