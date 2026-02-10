namespace Events.Api;

internal static class Constants
{
    internal const string Events = "Events";

    internal static class Routes
    {
        private const string BaseApi = "api";

        public const string Events = $"{BaseApi}/events";
        public const string EventById = $"{Events}/{{id:guid}}";
        public const string EventSchedule = $"{EventById}/schedule";
        public const string EventDetails = $"{EventById}/details";
        public const string EventPublish = $"{EventById}/publish";
    }
}
