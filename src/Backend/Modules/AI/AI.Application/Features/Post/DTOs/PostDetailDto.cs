namespace Marketing.Application.Posts.Dtos;


public class PostDetailDto
{
    public PostDetailDto() { } // AutoMapper requires this

    public Guid PostId { get; set; }
    public Guid EventId { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string? PromptUsed { get; set; }
    public string? AiModel { get; set; }
    public int? AiTokensUsed { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string TrackingToken { get; set; } = string.Empty;
    public string? ExternalPostUrl { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Permissions for UI
    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanPublish { get; set; }
    public bool CanArchive { get; set; }

    public IReadOnlyList<DistributionStatusDto> Distributions { get; set; } 
        = new List<DistributionStatusDto>();
}
