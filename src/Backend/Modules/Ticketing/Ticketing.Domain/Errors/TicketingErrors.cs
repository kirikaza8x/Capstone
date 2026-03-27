using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Errors;

public static class TicketingErrors
{
    public static class Order
    {
        public static Error NotFound(Guid orderId) => Error.NotFound(
            "Order.NotFound",
            $"Order '{orderId}' was not found.");

        public static Error CannotMarkPaid(OrderStatus currentStatus) => Error.Validation(
            "Order.CannotMarkPaid",
            $"Cannot mark order as paid. Current status is '{currentStatus}'.");

        public static Error CannotCancel(OrderStatus currentStatus) => Error.Validation(
            "Order.CannotCancel",
            $"Cannot cancel order. Current status is '{currentStatus}'.");

        public static readonly Error NoTickets = Error.Validation(
            "Order.NoTickets",
            "Order must contain at least one ticket.");

        public static readonly Error InvalidTotalPrice = Error.Validation(
            "Order.InvalidTotalPrice",
            "Total price must be greater than or equal to zero.");

        public static readonly Error CannotCancelWithUsedTickets = Error.Validation(
            "Order.CannotCancelWithUsedTickets",
            "Order cannot be cancelled because one or more tickets are already used.");

        public static readonly Error DuplicateVoucher = Error.Conflict(
            "Order.DuplicateVoucher",
            "Voucher has already been applied to this order.");

        public static readonly Error InvalidTicketSelection = Error.Validation(
            "Order.InvalidTicketSelection",
            "Invalid ticket selection.");

        public static readonly Error TicketNotPurchasable = Error.Conflict(
            "Order.TicketNotPurchasable",
            "Selected ticket is not purchasable at this time.");

        public static readonly Error SeatRequired = Error.Validation(
            "Order.SeatRequired",
            "SeatId is required for seat ticket type.");

        public static readonly Error SeatMustBeNullForZone = Error.Validation(
            "Order.SeatMustBeNullForZone",
            "SeatId must be null for zone ticket type.");

        public static readonly Error SeatNotFound = Error.NotFound(
            "Order.SeatNotFound",
            "Selected seat was not found.");

        public static readonly Error SeatNotBelongToArea = Error.Validation(
            "Order.SeatNotBelongToArea",
            "Selected seat does not belong to ticket type area.");

        public static readonly Error SeatNotAvailable = Error.Conflict(
            "Order.SeatNotAvailable",
            "Selected seat is not available.");

        public static readonly Error ZoneSoldOut = Error.Conflict(
            "Order.ZoneSoldOut",
            "Ticket zone is sold out.");

        public static readonly Error NotPaid = Error.Conflict(
            "Order.NotPaid",
            "Order is not in paid status.");

        public static readonly Error NotPending = Error.Conflict(
            "Order.NotPending",
            "Order is not in pending status.");
    }

    public static class Event
    {
        public static readonly Error NotOwner = Error.Forbidden(
            "Event.NotOwner",
            "You are not the owner of this event.");
    }

    public static class OrderTicket
    {
        public static Error NotFound(Guid orderTicketId) => Error.NotFound(
            "OrderTicket.NotFound",
            $"Order ticket '{orderTicketId}' was not found.");

        public static Error CannotUse(OrderTicketStatus currentStatus) => Error.Validation(
            "OrderTicket.CannotUse",
            $"Cannot check in ticket. Current status is '{currentStatus}'.");

        public static Error CannotCancel(OrderTicketStatus currentStatus) => Error.Validation(
            "OrderTicket.CannotCancel",
            $"Cannot cancel ticket. Current status is '{currentStatus}'.");

        public static readonly Error InvalidQrCode = Error.Validation(
            "OrderTicket.InvalidQrCode",
            "QR code is required.");

        public static readonly Error InvalidPrice = Error.Validation(
            "OrderTicket.InvalidPrice",
            "Price must be greater than or equal to 0.");
    }
    public static class Voucher
    {
        public static Error NotFound(string couponCode) => Error.NotFound(
            "Voucher.NotFound",
            $"Voucher with coupon code '{couponCode}' not found.");

        public static Error NotFound(Guid couponId) => Error.NotFound(
            "Voucher.NotFound",
            $"Voucher '{couponId}' not found.");

        public static readonly Error InvalidEvent = Error.Conflict(
            "Voucher.InvalidEvent",
            "This voucher is not applicable for this event.");

        public static Error InvalidName => Error.Validation(
            "Voucher.InvalidName",
            "The voucher name cannot be null or empty.");

        public static Error CouponCodeAlreadyExists(string couponCode) => Error.Conflict(
            "Voucher.NotFound",
            $"Voucher with coupon code '{couponCode}' not found.");

        public static readonly Error Expired = Error.Conflict(
            "Voucher.Expired",
            "Voucher is expired or not yet active.");

        public static readonly Error ExceededMaxUse = Error.Conflict(
            "Voucher.ExceededMaxUse",
            "Voucher has reached its usage limit.");

        public static readonly Error AlreadyUsedByUser = Error.Conflict(
            "Voucher.AlreadyUsedByUser",
            "You have already used this voucher.");

        public static readonly Error InvalidCouponCode = Error.Validation(
            "Voucher.InvalidCouponCode",
            "Coupon code is required.");

        public static readonly Error InvalidValue = Error.Validation(
            "Voucher.InvalidValue",
            "Voucher value must be greater than 0.");

        public static readonly Error InvalidMaxUse = Error.Validation(
            "Voucher.InvalidMaxUse",
            "Max use must be greater than 0.");

        public static readonly Error InvalidDateRange = Error.Validation(
            "Voucher.InvalidDateRange",
            "Start date must be before end date.");

        public static readonly Error CannotUpdateUsedVoucher = Error.Conflict(
            "Voucher.CannotUpdateUsedVoucher",
            "Cannot update voucher that has already been used.");

        public static readonly Error CannotDeleteUsedVoucher = Error.Conflict(
            "Voucher.CannotDeleteUsedVoucher",
            "Cannot delete voucher that has already been used.");

        public static readonly Error NotOwner = Error.Forbidden(
            "Voucher.NotOwner",
            "You are not the owner of this voucher.");
    }

    public static class CheckIn
    {
        public static Error InvalidQrCode => Error.Validation(
            "CheckIn.InvalidQrCode", "QR code is invalid.");

        public static Error TicketNotFound => Error.NotFound(
            "CheckIn.TicketNotFound", "Ticket not found.");

        public static Error SessionMismatch => Error.Validation(
            "CheckIn.SessionMismatch", "Ticket does not belong to current session.");

        public static Error AlreadyCheckedIn => Error.Conflict(
            "CheckIn.AlreadyCheckedIn", "Ticket has already been checked in.");

        public static Error TicketCancelled => Error.Conflict(
            "CheckIn.TicketCancelled", "Ticket has been cancelled.");

        public static Error InvalidTicketStatus => Error.Conflict(
            "CheckIn.InvalidTicketStatus", "Ticket status is invalid for check-in.");
    }
}
