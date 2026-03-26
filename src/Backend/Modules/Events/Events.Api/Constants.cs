using Microsoft.Extensions.Logging;

namespace Events.Api;

internal static class Constants
{
    internal static class Tags
    {
        public const string Events = "Events";
        public const string EventForStaff = "Events - Staff";
        public const string EventForAdmin = "Events - Admin";
        public const string EventForOrganizer = "Events - Organizer";

        public const string EventImages = "Event Images";
        public const string Sessions = "Sessions";
        public const string Areas = "Areas";
        public const string Seats = "Seats";
        public const string TicketTypes = "Ticket Types";
        public const string Quotas = "Session Ticket Quotas";
        public const string Member = "Event Member";
        public const string Categories = "Categories";
        public const string Hashtags = "Hashtags";
        public const string Search = "Search & Discovery";
    }

    internal static class Routes
    {
        private const string BaseApi = "api";
        private const string OrganizerBaseApi = $"{BaseApi}/organizer";

        // Organizer
        public const string OrganizerEvents = $"{OrganizerBaseApi}/events";
        public const string OrganizerEventById = $"{OrganizerEvents}/{{eventId:guid}}";
        public const string OrganizerEventByUrlPath = $"{OrganizerEvents}/url/{{urlPath}}";
        public const string ExportEventMembers = $"{OrganizerBaseApi}/events/{{eventId:guid}}/members/export";
        public const string OrganizerTicketTypes = $"{OrganizerEventById}/ticket-types";
        public const string OrganizerTicketTypeById = $"{OrganizerTicketTypes}/{{ticketTypeId:guid}}";
        public const string OrganizerSessions = $"{OrganizerEventById}/sessions";
        public const string OrganizerSessionById = $"{OrganizerSessions}/{{sessionId:guid}}";
        public const string OrganizerEventMember = $"{OrganizerEventById}/member";
        public const string OrganizerEventMemberById = $"{OrganizerEventMember}/{{memberId:guid}}";
        public const string OrganizerEventImages = $"{OrganizerEventById}/images";
        public const string OrganizerEventImageById = $"{OrganizerEventImages}/{{imageId:guid}}";

        // Admin
        public const string AdminEvents = $"{BaseApi}/admin/events";

        // Staff
        public const string StaffEvents = $"{BaseApi}/staff/events";
        public const string StaffEventById = $"{BaseApi}/staff/events/{{eventId:guid}}";

        // Events
        public const string Events = $"{BaseApi}/events";
        public const string EventById = $"{Events}/{{eventId:guid}}";
        public const string EventByUrlPath = $"{Events}/url/{{urlPath}}";

        // Images
        public const string EventImages = $"{EventById}/images";
        public const string EventImageById = $"{EventImages}/{{imageId:guid}}";

        // Sessions
        public const string Sessions = $"{EventById}/sessions";
        public const string SessionById = $"{Sessions}/{{sessionId:guid}}";

        // Ticket Types
        public const string TicketTypes = $"{EventById}/ticket-types";
        public const string TicketTypeById = $"{TicketTypes}/{{ticketTypeId:guid}}";

        // Areas
        public const string Areas = $"{EventById}/areas";
        public const string AreaById = $"{Areas}/{{areaId:guid}}";

        // Seats
        public const string Seats = $"{AreaById}/seats";
        public const string SeatById = $"{Seats}/{{seatId:guid}}";

        // Session Ticket Quotas
        public const string Quotas = $"{SessionById}/quotas";
        public const string QuotaByTicketType = $"{Quotas}/{{ticketTypeId:guid}}";

        // Categories
        public const string Categories = $"{BaseApi}/categories";
        public const string CategoryById = $"{Categories}/{{categoryId:int}}";

        // Hashtags
        public const string Hashtags = $"{BaseApi}/hashtags";
        public const string HashtagById = $"{Hashtags}/{{hashtagId:int}}";

        // Search
        public const string Search = $"{Events}/search";
        public const string Trending = $"{Events}/trending";
        public const string Upcoming = $"{Events}/upcoming";

    }
}
