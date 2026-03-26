namespace Events.PublicApi.Constants;

public static class EventMemberPermission
{
    public const string CheckIn = "CheckIn";
    public const string ViewReports = "ViewReports";

    public static readonly IReadOnlyList<string> All = [CheckIn, ViewReports];
}
