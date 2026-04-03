using Marketing.Domain.Enums;
using Shared.Domain.DDD;

namespace Marketing.Domain.Entities;

/// <summary>
/// Child entity representing the distribution of a PostMarketing 
/// to a specific external platform (Facebook, LinkedIn, etc.).
/// 
/// This entity is owned by the PostMarketing aggregate root and 
/// cannot exist independently. It is accessed and modified only 
/// through the aggregate's methods.
/// </summary>
public sealed class ExternalDistribution : Entity<Guid>
{
    // =========================================================
    // Properties
    // =========================================================

    public ExternalPlatform Platform { get; private set; }

    public string ExternalUrl { get; private set; } = string.Empty;

    public string? ExternalPostId { get; private set; }

    public string? PlatformMetadata { get; private set; }

    public DistributionStatus Status { get; private set; } = DistributionStatus.Pending;

    public DateTime? SentAt { get; private set; }

    public string? ErrorMessage { get; private set; }

    // ── FK to Parent Aggregate ──
    public Guid PostMarketingId { get; private set; }

    // =========================================================
    // Constructors
    // =========================================================

    private ExternalDistribution() { }

    private ExternalDistribution(
        Guid id,
        Guid postMarketingId,
        ExternalPlatform platform,
        string externalUrl,
        string? externalPostId = null,
        string? platformMetadata = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Distribution Id cannot be empty", nameof(id));

        if (postMarketingId == Guid.Empty)
            throw new ArgumentException("PostMarketingId is required", nameof(postMarketingId));

        if (platform == ExternalPlatform.Unknown)
            throw new ArgumentException("Platform must be specified and cannot be Unknown", nameof(platform));

        if (!string.IsNullOrWhiteSpace(externalUrl))
            ExternalUrl = externalUrl.Trim();

        Id = id;
        PostMarketingId = postMarketingId;
        Platform = platform;
        ExternalPostId = externalPostId?.Trim();
        PlatformMetadata = platformMetadata?.Trim();
    }

    // =========================================================
    // Factory Method
    // =========================================================

    public static ExternalDistribution Create(
        Guid postMarketingId,
        ExternalPlatform platform,
        string externalUrl = "",
        string? externalPostId = null,
        string? platformMetadata = null)
    {
        return new ExternalDistribution(
            Guid.NewGuid(),
            postMarketingId,
            platform,
            externalUrl,
            externalPostId,
            platformMetadata);
    }

    // =========================================================
    // State Transition Methods (Internal - Aggregate Only)
    // =========================================================

    internal void MarkAsSent()
    {
        if (Status == DistributionStatus.Sent)
            return;

        Status = DistributionStatus.Sent;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    internal void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        if (Status == DistributionStatus.Failed)
            return;

        Status = DistributionStatus.Failed;
        ErrorMessage = errorMessage.Trim();
    }

    internal void UpdateExternalUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("External URL cannot be empty", nameof(newUrl));

        ExternalUrl = newUrl.Trim();
    }

    internal void MarkAsInProgress()
    {
        // Don't downgrade or override terminal states
        if (Status is DistributionStatus.Sent or DistributionStatus.Failed)
            return;

        Status = DistributionStatus.InProgress;
        // Keep SentAt null until platform confirms
    }

    internal void UpdateMetadata(string? externalPostId, string? platformMetadata)
    {
        ExternalPostId = externalPostId?.Trim();
        PlatformMetadata = platformMetadata?.Trim();
    }

    // =========================================================
    // Query Methods (Public - Read-Only)
    // =========================================================

    public bool IsSent() => Status == DistributionStatus.Sent;

    public bool IsFailed() => Status == DistributionStatus.Failed;

    public bool CanRetry() => Status == DistributionStatus.Failed;

    public bool IsPending() => Status == DistributionStatus.Pending;

    public bool IsInProgress() => Status == DistributionStatus.InProgress;
}