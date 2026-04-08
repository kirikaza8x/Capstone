namespace Marketing.Application.Posts.Dtos;

public class FacebookOperationResult
{
    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The external post ID from your system
    /// </summary>
    public string? ExternalPostId { get; set; }

    /// <summary>
    /// The Facebook-assigned post ID (e.g., "pageId_postId")
    /// </summary>
    public string? FacebookPostId { get; set; }

    /// <summary>
    /// Public URL to the Facebook post
    /// </summary>
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Whether the operation can be retried (matches n8n can_retry field)
    /// </summary>
    public bool CanRetry { get; set; } = true;

    /// <summary>
    /// Operation type: "create", "update", or "delete"
    /// </summary>
    public string? Operation { get; set; }

    /// <summary>
    /// Additional platform-specific metadata as JSON string
    /// Matches n8n platform_metadata field
    /// </summary>
    public string? PlatformMetadata { get; set; }
}