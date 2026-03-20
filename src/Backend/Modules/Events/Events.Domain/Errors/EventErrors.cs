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
            $"Cannot publish event. Current status is '{currentStatus}'.");

        public static readonly Error NoSessions = Error.Validation(
            "Event.NoSessions",
            "Cannot publish event. At least one session is required.");

        public static readonly Error NoTicketTypes = Error.Validation(
            "Event.NoTicketTypes",
            "Cannot publish event. At least one ticket type is required.");

        public static readonly Error TicketTypeNotAssignedToArea = Error.Validation(
            "Event.TicketTypeNotAssignedToArea",
            "Cannot publish event. All ticket types must be assigned to an area.");

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
            $"Cannot unpublish event. Current status is '{currentStatus}'.");

        public static Error CannotCancel(EventStatus currentStatus) => Error.Validation(
            "Event.CannotCancel",
            $"Cannot cancel event. Current status is '{currentStatus}'.");

        public static Error CannotRequestCancellation(EventStatus currentStatus) => Error.Validation(
            "Event.CannotRequestCancellation",
            $"Cannot request cancellation. Current status is '{currentStatus}'.");

        public static Error CannotRequestPublish(EventStatus currentStatus) => Error.Validation(
            "Event.CannotRequestPublish",
            $"Cannot request publish. Current status is '{currentStatus}'.");

        public static Error CannotDelete(EventStatus currentStatus) => Error.Validation(
            "Event.CannotDelete",
            $"Cannot delete event. Current status is '{currentStatus}'.");

        public static Error UrlPathTooShort() => Error.Validation(
            "Event.UrlPathTooShort",
            "The URL path must be at least 3 characters long.");

        public static Error UrlPathTooLong() => Error.Validation(
            "Event.UrlPathTooLong",
            "The URL path must not exceed 100 characters.");

        public static Error InvalidUrlPathFormat(string urlPath) => Error.Validation(
            "Event.InvalidUrlPathFormat",
            $"The URL path '{urlPath}' contains invalid characters.");

        public static readonly Error AlreadyStarted = Error.Validation(
            "Event.AlreadyStarted",
            "Cannot process request. The event has already started.");

        public static Error CannotUpdate(EventStatus currentStatus) => Error.Validation(
            "Event.CannotUpdate",
            $"Cannot update event. Current status is '{currentStatus}'. Only draft events can be updated.");

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

        public static Error CannotComplete(EventStatus currentStatus) => Error.Validation(
            "Event.CannotComplete",
            $"Cannot complete event. Current status is '{currentStatus}'.");

        public static readonly Error CannotCompleteBeforeEnd = Error.Validation(
            "Event.CannotCompleteBeforeEnd",
            "Cannot complete event before event end time.");

        public static Error CannotTriggerReminder(EventStatus currentStatus) => Error.Validation(
            "Event.CannotTriggerReminder",
            $"Cannot trigger reminder. Current status is '{currentStatus}'.");

        public static readonly Error EmailReminderDisabled = Error.Validation(
            "Event.EmailReminderDisabled",
            "Email reminder is disabled for this event.");

        public static readonly Error ReminderNotDue = Error.Validation(
            "Event.ReminderNotDue",
            "Reminder is not due yet.");

        public static Error CannotRejectPublishRequest(EventStatus currentStatus) => Error.Validation(
            "Event.CannotRejectPublishRequest",
            $"Cannot reject publish request. Current status is '{currentStatus}'.");

        public static Error CannotRejectCancellationRequest(EventStatus currentStatus) => Error.Validation(
            "Event.CannotRejectCancellationRequest",
            $"Cannot reject cancellation request. Current status is '{currentStatus}'.");

        public static readonly Error RejectReasonRequired = Error.Validation(
            "Event.RejectReasonRequired",
            "Reject reason is required.");

        public static Error CannotSuspend(EventStatus currentStatus) => Error.Validation(
            "Event.CannotSuspend",
            $"Cannot suspend event. Current status is '{currentStatus}'.");

        public static readonly Error SuspendReasonRequired = Error.Validation(
            "Event.SuspendReasonRequired",
            "Suspend reason is required.");

        public static readonly Error CannotSuspendAfterStart = Error.Validation(
            "Event.CannotSuspendAfterStart",
            "Cannot suspend event after event start time.");

        public static readonly Error InvalidSuspendFixWindow = Error.Validation(
            "Event.InvalidSuspendFixWindow",
            "Suspend fix window must be greater than zero.");

        public static readonly Error CannotResubmitAfterSuspendDeadline = Error.Validation(
            "Event.CannotResubmitAfterSuspendDeadline",
            "Cannot re-submit because suspension fix deadline has passed.");

        public static readonly Error EventStartMustBeInFuture = Error.Validation(
            "Event.EventStartMustBeInFuture",
            "Event start time must be in the future.");

        public static readonly Error SuspendByRequired = Error.Validation(
            "Event.SuspendByRequired",
            "Suspended by is required.");
    }


    public static class EventMemberErrors
    {
        public static Error NotFound(Guid memberId) => Error.NotFound(
            "EventMember.NotFound",
            $"The event member with ID '{memberId}' was not found.");

        public static Error AlreadyExists(string email) => Error.Conflict(
            "EventMember.AlreadyExists",
            $"User '{email}' is already a member of this event.");

        public static readonly Error UserNotEligible = Error.Validation(
            "EventMember.UserNotEligible",
            "Only users with Attendee role can be added as event members.");

        public static Error UserNotFound(string email) => Error.NotFound(
            "EventMember.UserNotFound",
            $"No registered user found with email '{email}'.");
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

        public static Error AreaNotBelongToEvent(Guid areaId, Guid eventId) => Error.Validation(
            "TicketType.AreaNotBelongToEvent",
            $"Area '{areaId}' does not belong to event '{eventId}'.");

        public static readonly Error SoldOut = Error.Conflict(
            "TicketType.SoldOut",
            "The ticket type is sold out.");

        public static Error NotEnoughTickets(int requested, int available) => Error.Validation(
            "TicketType.NotEnoughTickets",
            $"Requested {requested} tickets but only {available} available.");

        public static readonly Error InvalidSoldQuantityAmount = Error.Validation(
            "TicketType.InvalidSoldQuantityAmount",
            "Sold quantity amount must be greater than zero.");

        public static readonly Error CannotDecreaseSoldBelowZero = Error.Validation(
            "TicketType.CannotDecreaseSoldBelowZero",
            "Cannot decrease sold quantity below zero.");

        public static Error ExceedQuantity(int quantity, int attemptedSold) => Error.Conflict(
            "TicketType.ExceedQuantity",
            $"Sold quantity cannot exceed total quantity. Quantity: {quantity}, attempted sold: {attemptedSold}.");
    }

    public static class SessionTicketQuotaErrors
    {
        public static Error NotFound(Guid sessionId, Guid ticketTypeId) => Error.NotFound(
            "SessionTicketQuota.NotFound",
            $"No quota found for session '{sessionId}' and ticket type '{ticketTypeId}'.");

        public static Error TicketTypeNotZone() => Error.Validation(
            "SessionTicketQuota.TicketTypeNotZone",
            "Session ticket quota only applies to zone-type areas.");
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

        public static Error InUse(int categoryId) => Error.Validation(
            "Category.InUse",
            $"Cannot delete category '{categoryId}' because it is being used by one or more events.");
    }

    public static class HashtagErrors
    {
        public static Error NotFound(int hashtagId) => Error.NotFound(
            "Hashtag.NotFound",
            $"The hashtag with ID '{hashtagId}' was not found.");

        public static Error SlugAlreadyExists(string slug) => Error.Conflict(
            "Hashtag.SlugAlreadyExists",
            $"A hashtag with slug '{slug}' already exists.");

        public static Error InUse(int hashtagId) => Error.Validation(
            "Hashtag.InUse",
            $"Cannot delete hashtag '{hashtagId}' because it is being used by one or more events.");
    }
}
