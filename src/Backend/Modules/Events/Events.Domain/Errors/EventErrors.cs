using Shared.Domain.Abstractions;

namespace Events.Domain.Errors;

public static class EventErrors
{
    public static class Event
    {
        public static Error NotFound(Guid eventId) => Error.NotFound(
            "Event.NotFound",
            $"The event with ID '{eventId}' was not found.");

        public static Error UrlPathAlreadyExists(string urlPath) => Error.Conflict(
            "Event.UrlPathAlreadyExists",
            $"An event with URL path '{urlPath}' already exists.");
    }

    public static class EventSession
    {
        public static Error NotFound(Guid sessionId) => Error.NotFound(
            "EventSession.NotFound",
            $"The event session with ID '{sessionId}' was not found.");
    }

    public static class TicketType
    {
        public static Error NotFound(Guid ticketTypeId) => Error.NotFound(
            "TicketType.NotFound",
            $"The ticket type with ID '{ticketTypeId}' was not found.");

        public static readonly Error SoldOut = Error.Conflict(
            "TicketType.SoldOut",
            "The ticket type is sold out.");

        public static Error NotEnoughTickets(int requested, int available) => Error.Validation(
            "TicketType.NotEnoughTickets",
            $"Requested {requested} tickets but only {available} available.");
    }

    public static class Area
    {
        public static Error NotFound(Guid areaId) => Error.NotFound(
            "Area.NotFound",
            $"The area with ID '{areaId}' was not found.");
    }

    public static class Seat
    {
        public static readonly Error NotAvailable = Error.Conflict(
            "Seat.NotAvailable",
            "The seat is not available.");
    }
}