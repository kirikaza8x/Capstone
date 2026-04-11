namespace AI.PublicApi.Enums;

public enum TargetType
{
    Event,
    EventList,
    Ticket,
    Voucher,
    Organizer,
    Category
}

public static class TargetTypeExtensions
{
    public static string ToValue(this TargetType targetType) => targetType switch
    {
        TargetType.Event => "event",
        TargetType.EventList => "event_list",
        TargetType.Ticket => "ticket",
        TargetType.Voucher => "voucher",
        TargetType.Organizer => "organizer",
        TargetType.Category => "category",
        _ => "unknown"
    };
}
