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
    
    /// <summary>
    /// The target platform for this distribution
    /// </summary>
    public ExternalPlatform Platform { get; private set; }
    
    /// <summary>
    /// The public URL of the post on the external platform.
    /// Example: https://facebook.com/YourPage/posts/123456789
    /// </summary>
    public string ExternalUrl { get; private set; } = string.Empty;
    
    /// <summary>
    /// Platform-specific post ID returned by the platform's API.
    /// Used for analytics, updates, or deletion via API.
    /// Example: Facebook post ID "123456789_987654321"
    /// </summary>
    public string? ExternalPostId { get; private set; }
    
    /// <summary>
    /// Additional platform-specific metadata stored as JSON string.
    /// Examples:
    /// - Facebook: {"thread_id":"xyz","is_scheduled":true}
    /// - LinkedIn: {"urn":"urn:li:share:123456"}
    /// - TikTok: {"video_id":"7123456789","draft":false}
    /// </summary>
    public string? PlatformMetadata { get; private set; }
    
    /// <summary>
    /// Current status of this distribution
    /// </summary>
    public DistributionStatus Status { get; private set; } = DistributionStatus.Pending;
    
    /// <summary>
    /// UTC timestamp when the distribution request was successfully 
    /// sent to the external platform (via n8n or direct API)
    /// </summary>
    public DateTime? SentAt { get; private set; }
    
    /// <summary>
    /// Error message from the platform or n8n if distribution failed.
    /// Useful for debugging and user feedback.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // =========================================================
    // Constructors
    // =========================================================
    
    /// <summary>
    /// Private parameterless constructor for EF Core materialization.
    /// Do not use directly in application code.
    /// </summary>
    private ExternalDistribution() { }

    /// <summary>
    /// Private constructor used by factory method.
    /// Enforces invariants at creation time.
    /// </summary>
    private ExternalDistribution(
        Guid id,
        ExternalPlatform platform,
        string externalUrl,
        string? externalPostId = null,
        string? platformMetadata = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Distribution Id cannot be empty", nameof(id));
            
        if (platform == ExternalPlatform.Unknown)
            throw new ArgumentException("Platform must be specified and cannot be Unknown", nameof(platform));
        
        // ExternalUrl can be empty at creation (will be set later via callback)
        // but if provided, it must be valid
        if (!string.IsNullOrWhiteSpace(externalUrl))
        {
            ExternalUrl = externalUrl.Trim();
        }
        
        Id = id;
        Platform = platform;
        ExternalPostId = externalPostId?.Trim();
        PlatformMetadata = platformMetadata?.Trim();
    }

    // =========================================================
    // Factory Method
    // =========================================================
    
    /// <summary>
    /// Creates a new ExternalDistribution entity in Pending status.
    /// Call this from within the PostMarketing aggregate.
    /// </summary>
    /// <param name="platform">Target platform (cannot be Unknown)</param>
    /// <param name="externalUrl">Initial external URL (can be empty, set later via callback)</param>
    /// <param name="externalPostId">Optional platform-specific post ID</param>
    /// <param name="platformMetadata">Optional JSON metadata for platform-specific data</param>
    /// <returns>New ExternalDistribution instance</returns>
    public static ExternalDistribution Create(
        ExternalPlatform platform,
        string externalUrl = "",
        string? externalPostId = null,
        string? platformMetadata = null)
    {
        return new ExternalDistribution(
            Guid.NewGuid(),
            platform,
            externalUrl,
            externalPostId,
            platformMetadata);
    }

    // =========================================================
    // State Transition Methods (Internal - Aggregate Only)
    // =========================================================
    
    /// <summary>
    /// Marks this distribution as successfully sent to the platform.
    /// Called by PostMarketing aggregate after receiving n8n callback.
    /// </summary>
    internal void MarkAsSent()
    {
        // Idempotent: allow multiple calls without side effects
        if (Status == DistributionStatus.Sent)
            return;
            
        Status = DistributionStatus.Sent;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null; // Clear any previous errors
    }

    /// <summary>
    /// Marks this distribution as failed with an error message.
    /// Called by PostMarketing aggregate after receiving failure callback.
    /// </summary>
    /// <param name="errorMessage">Description of what went wrong</param>
    internal void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));
            
        // Idempotent: allow multiple calls without side effects
        if (Status == DistributionStatus.Failed)
            return;
            
        Status = DistributionStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        // Keep SentAt null or unchanged - it wasn't successfully sent
    }

    /// <summary>
    /// Updates the external URL after successful distribution.
    /// </summary>
    /// <param name="newUrl">The confirmed URL on the external platform</param>
    internal void UpdateExternalUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("External URL cannot be empty", nameof(newUrl));
            
        ExternalUrl = newUrl.Trim();
    }

    /// <summary>
    /// Updates platform-specific identifiers and metadata.
    /// </summary>
    /// <param name="externalPostId">Platform's post ID</param>
    /// <param name="platformMetadata">JSON metadata string</param>
    internal void UpdateMetadata(string? externalPostId, string? platformMetadata)
    {
        ExternalPostId = externalPostId?.Trim();
        PlatformMetadata = platformMetadata?.Trim();
    }

    // =========================================================
    // Query Methods (Public - Read-Only)
    // =========================================================
    
    /// <summary>
    /// Returns true if this distribution was successfully sent
    /// </summary>
    public bool IsSent() => Status == DistributionStatus.Sent;
    
    /// <summary>
    /// Returns true if this distribution failed
    /// </summary>
    public bool IsFailed() => Status == DistributionStatus.Failed;
    
    /// <summary>
    /// Returns true if this distribution can be retried (currently failed)
    /// </summary>
    public bool CanRetry() => Status == DistributionStatus.Failed;
    
    /// <summary>
    /// Returns true if this distribution is still pending
    /// </summary>
    public bool IsPending() => Status == DistributionStatus.Pending;
}