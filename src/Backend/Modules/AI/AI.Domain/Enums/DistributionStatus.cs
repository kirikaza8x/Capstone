namespace Marketing.Domain.Enums;

public enum DistributionStatus
{
    /// <summary>
    /// Queued in our system, not yet sent to n8n
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Successfully sent to n8n, awaiting platform confirmation
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Confirmed posted to external platform (via n8n callback)
    /// </summary>
    Sent = 2,
    
    /// <summary>
    /// Distribution failed (can retry)
    /// </summary>
    Failed = 3
}