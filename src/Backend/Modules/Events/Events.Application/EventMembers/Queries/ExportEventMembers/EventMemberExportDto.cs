namespace Events.Application.EventMembers.Queries.ExportEventMembers;

public class EventMemberExportDto
{
    public int Index { get; set; }
    public Guid Id { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string IsActive { get; set; } = string.Empty;
}
