using Shared.Domain.Abstractions;

namespace Events.Domain.Errors;

public static class EventErrors
{
    public static class Event
    {
        public static readonly Error NotFound = Error.NotFound(
            "Event.NotFound",
            "The event was not found.");

        public static readonly Error AlreadyPublished = Error.Conflict(
            "Event.AlreadyPublished",
            "The event has already been published.");

        public static readonly Error CannotPublishDraft = Error.Validation(
            "Event.CannotPublishDraft",
            "Only draft events can be published.");

        public static readonly Error CannotClose = Error.Validation(
            "Event.CannotClose",
            "Only published events can be closed.");
    }

    public static class Seat
    {
        public static readonly Error NotAvailable = Error.Conflict(
            "Seat.NotAvailable",
            "The seat is not available for reservation.");

        public static readonly Error NotReserved = Error.Validation(
            "Seat.NotReserved",
            "The seat must be reserved before selling.");
    }

    public static class Area
    {
        public static readonly Error NotFound = Error.NotFound(
            "Area.NotFound",
            "The area was not found.");
    }
}