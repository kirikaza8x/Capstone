namespace Users.PublicApi.Constants;

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string Staff = nameof(Staff);
    public const string Organizer = nameof(Organizer);
    public const string Attendee = nameof(Attendee);

    public static readonly string[] All = new[]
    {
        Admin,
        Staff,
        Organizer,
        Attendee
    };

    public static readonly string[] AllExceptAttendee = new[]
    {
        Admin,
        Staff,
        Organizer
    };

    public static readonly string[] AdminAndStaff = new[]
    {
        Admin,
        Staff
    };
}