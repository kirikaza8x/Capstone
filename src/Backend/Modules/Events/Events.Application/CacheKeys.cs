namespace Events.Application;

public static class CacheKeys
{
    public static class Events
    {
        public const string Prefix = "Events";

        public static string GetById(Guid id) => $"{Prefix}:Detail:{id}";

        public static string GetList(int page, int size, string? sortCol, object? sortDir) =>
            $"{Prefix}:List:P{page}:S{size}:Col_{sortCol}:Dir_{sortDir}";
    }
}
