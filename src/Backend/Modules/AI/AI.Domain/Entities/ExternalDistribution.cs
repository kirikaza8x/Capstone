// ExternalDistribution.cs
using Marketing.Domain.Enums;
using Shared.Domain.DDD;

namespace Marketing.Domain.Entities;

/// <summary>
/// Child entity representing the distribution of a PostMarketing
/// to a specific external platform (Facebook, LinkedIn, etc.).
/// Owned by the PostMarketing aggregate — never modified directly.
/// </summary>
public sealed class ExternalDistribution : Entity<Guid>
{
    public ExternalPlatform Platform { get; private set; }
    public string ExternalUrl { get; private set; } = string.Empty;
    public string? ExternalPostId { get; private set; }
    public string? PlatformMetadata { get; private set; }
    public DistributionStatus Status { get; private set; } = DistributionStatus.Pending;
    public DateTime? SentAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Guid PostMarketingId { get; private set; }

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
            throw new ArgumentException("Platform must be specified", nameof(platform));

        if (!string.IsNullOrWhiteSpace(externalUrl))
            ExternalUrl = externalUrl.Trim();

        Id = id;
        PostMarketingId = postMarketingId;
        Platform = platform;
        ExternalPostId = externalPostId?.Trim();
        PlatformMetadata = platformMetadata?.Trim();
    }

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

    // ── State Transitions (internal — aggregate only) ──

    internal void MarkAsSent()
    {
        if (Status == DistributionStatus.Sent) return;
        Status = DistributionStatus.Sent;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    internal void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        if (Status == DistributionStatus.Failed) return;
        Status = DistributionStatus.Failed;
        ErrorMessage = errorMessage.Trim();
    }

    internal void UpdateExternalUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("External URL cannot be empty", nameof(newUrl));

        ExternalUrl = newUrl.Trim();
    }

    internal void UpdateMetadata(string? externalPostId, string? platformMetadata)
    {
        ExternalPostId = externalPostId?.Trim();
        PlatformMetadata = platformMetadata?.Trim();
    }

    // ── Query Methods ──

    public bool IsSent() => Status == DistributionStatus.Sent;
    public bool IsFailed() => Status == DistributionStatus.Failed;
    public bool IsPending() => Status == DistributionStatus.Pending;
    public bool CanRetry() => Status == DistributionStatus.Failed;
}