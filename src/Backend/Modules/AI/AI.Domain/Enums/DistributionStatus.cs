namespace Marketing.Domain.Enums;

/// <summary>
/// Status of a post distribution to an external platform.
/// </summary>
public enum DistributionStatus
{
    /// <summary>
    /// Distribution is queued/pending, not yet sent to platform
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Successfully sent and confirmed on external platform
    /// </summary>
    Sent = 1,
    
    /// <summary>
    /// Distribution failed (can be retried)
    /// </summary>
    Failed = 2
}