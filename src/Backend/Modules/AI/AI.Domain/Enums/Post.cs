namespace Marketing.Domain.Enums;

public enum PostStatus
{
    /// <summary>Created by organizer or AI, not yet submitted. Editable.</summary>
    Draft,

    /// <summary>Submitted for admin review. Not editable until decision.</summary>
    Pending,

    /// <summary>Admin approved. Organizer may now publish to the platform.</summary>
    Approved,

    /// <summary>Admin rejected. Organizer may edit and resubmit.</summary>
    Rejected,

    /// <summary>Live on AIPromo platform. Visible to attendees.</summary>
    Published,

    /// <summary>Removed from platform (by organizer or admin force-remove).</summary>
    Archived,
}

public enum FacebookPeriod
{
    Day,

    Week,

    days_28
}