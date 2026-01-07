namespace Shared.Infrastructure.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty; 

    public DateTime OccurredOnUtc { get; init; }       
    
    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }
}
