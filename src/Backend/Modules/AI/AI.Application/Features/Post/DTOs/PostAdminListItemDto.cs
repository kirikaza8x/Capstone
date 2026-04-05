namespace Marketing.Application.Posts.Dtos;

public class PostAdminListItemDto(
    Guid PostId,
    Guid EventId,
    string EventTitle,
    Guid OrganizerId,
    string OrganizerName,
    string Title,
    string Status,
    string Platform,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? RejectionReason,
    IReadOnlyList<DistributionStatusDto> Distributions
)
{
    public Guid PostId { get; init; } = PostId;
    public Guid EventId { get; init; } = EventId;
    public string EventTitle { get; init; } = EventTitle;
    public Guid OrganizerId { get; init; } = OrganizerId;
    public string OrganizerName { get; init; } = OrganizerName;
    public string Title { get; init; } = Title;
    public string Status { get; init; } = Status;
    public string Platform { get; init; } = Platform;
    public DateTime SubmittedAt { get; init; } = SubmittedAt;
    public DateTime? ReviewedAt { get; init; } = ReviewedAt;
    public string? RejectionReason { get; init; } = RejectionReason;
    public IReadOnlyList<DistributionStatusDto> Distributions { get; init; } = Distributions;
}
