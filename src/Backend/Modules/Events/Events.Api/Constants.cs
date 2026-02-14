namespace Events.Api;

internal static class Constants
{
    internal const string Events = "Events";

    internal static class Routes
    {
        private const string BaseApi = "api";

        // Events
        public const string Events = $"{BaseApi}/events";
        public const string EventById = $"{Events}/{{eventId:guid}}";

        // Sessions
        public const string EventSessions = $"{EventById}/sessions";
        public const string SessionById = $"{EventSessions}/{{sessionId:guid}}";

        // Ticket Types
        public const string TicketTypes = $"{SessionById}/ticket-types";
        public const string TicketTypeById = $"{TicketTypes}/{{ticketTypeId:guid}}";

        // Areas
        public const string Areas = $"{EventById}/areas";
        public const string AreaById = $"{Areas}/{{areaId:guid}}";

        // Seats
        public const string Seats = $"{AreaById}/seats";
        public const string SeatById = $"{Seats}/{{seatId:guid}}";

        // Images
        public const string EventImages = $"{EventById}/images";
        public const string EventImageById = $"{EventImages}/{{imageId:guid}}";

    }
}
