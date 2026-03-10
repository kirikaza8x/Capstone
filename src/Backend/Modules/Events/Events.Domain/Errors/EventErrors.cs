using Events.Domain.Enums;
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

        public static Error CannotPublish(EventStatus currentStatus) => Error.Validation(
            "Event.CannotPublish",
            $"Cannot publish event. Current status is '{currentStatus}'. Only draft or pending events can be published.");

        public static readonly Error NoSessions = Error.Validation(
            "Event.NoSessions",
            "Cannot publish event. At least one session is required.");

        public static readonly Error SessionHasNoTicketTypes = Error.Validation(
            "Event.SessionHasNoTicketTypes",
            "Cannot publish event. Every session must have at least one ticket type.");

        public static readonly Error MissingSchedule = Error.Validation(
            "Event.MissingSchedule",
            "Cannot publish event. Event schedule (start/end time, ticket sale period) is required.");

        public static readonly Error InvalidTicketSalePeriod = Error.Validation(
            "Event.InvalidTicketSalePeriod",
            "Cannot publish event. Ticket sale end time must be before the event end time.");

        public static Error CannotUnpublish(EventStatus currentStatus) => Error.Validation(
            "Event.CannotUnpublish",
            $"Cannot unpublish event. Current status is '{currentStatus}'. Only published events can be unpublished.");

        public static Error CannotCancel(EventStatus currentStatus) => Error.Validation(
            "Event.CannotCancel",
            $"Cannot cancel event. Current status is '{currentStatus}'. Completed or already cancelled events cannot be cancelled.");

        public static Error CannotRequestCancellation(EventStatus currentStatus) => Error.Validation(
            "Event.CannotRequestCancellation",
            $"Cannot request cancellation. Current status is '{currentStatus}'. Only published or unpublished events can request cancellation.");

        public static Error CannotDelete(EventStatus currentStatus) => Error.Validation(
            "Event.CannotDelete",
            $"Cannot delete event. Current status is '{currentStatus}'. Only draft events can be deleted.");

        public static Error UrlPathTooShort() => Error.Validation(
            "Event.UrlPathTooShort",
            "The URL path must be at least 3 characters long.");

        public static Error UrlPathTooLong() => Error.Validation(
            "Event.UrlPathTooLong",
            "The URL path must not exceed 100 characters.");

        public static Error InvalidUrlPathFormat(string urlPath) => Error.Validation(
            "Event.InvalidUrlPathFormat",
            $"The URL path '{urlPath}' contains invalid characters. Only lowercase letters, numbers, and hyphens are allowed.");

        public static readonly Error AlreadyStarted = Error.Validation(
            "Event.AlreadyStarted",
            "Cannot process request. The event has already started.");

        public static readonly Error NotOwner = Error.Forbidden(
            "Event.NotOwner",
            "You are not the owner of this event.");

        public static readonly Error HasPaidOrders = Error.Conflict(
            "Event.HasPaidOrders",
            "Cannot cancel event. There are paid orders associated with this event.");

        public static readonly Error InvalidSpec = Error.Validation(
            "Event.InvalidSpec",
            "The spec JSON is invalid or cannot be parsed.");

        public static readonly Error SpecHasNoAreas = Error.Validation(
            "Event.SpecHasNoAreas",
            "The spec must contain at least one area.");
    }

    public static class EventSessionErrors
    {
        public static Error NotFound(Guid sessionId) => Error.NotFound(
            "EventSession.NotFound",
            $"The event session with ID '{sessionId}' was not found.");
    }

    public static class TicketTypeErrors
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

    public static class EventImageErrors
    {
        public static Error NotFound(Guid imageId) => Error.NotFound(
            "EventImage.NotFound",
            $"The event image with ID '{imageId}' was not found.");

        public static Error InvalidFileType() => Error.Validation(
            "EventImage.InvalidFileType",
            "Invalid file type. Allowed: JPEG, PNG, GIF, WebP.");

        public static Error FileTooLarge(long maxSizeInMb) => Error.Validation(
            "EventImage.FileTooLarge",
            $"File size exceeds {maxSizeInMb}MB limit.");

        public static Error FileRequired() => Error.Validation(
            "EventImage.FileRequired",
            "File is required.");
    }

    public static class CategoryErrors
    {
        public static Error NotFound(int categoryId) => Error.NotFound(
            "Category.NotFound",
            $"The category with ID '{categoryId}' was not found.");

        public static Error CodeAlreadyExists(string code) => Error.Conflict(
            "Category.CodeAlreadyExists",
            $"A category with code '{code}' already exists.");
    }

    public static class HashtagErrors
    {
        public static Error NotFound(int hashtagId) => Error.NotFound(
            "Hashtag.NotFound",
            $"The hashtag with ID '{hashtagId}' was not found.");

        public static Error SlugAlreadyExists(string slug) => Error.Conflict(
            "Hashtag.SlugAlreadyExists",
            $"A hashtag with slug '{slug}' already exists.");
    }
}