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
    }

    public static class Voucher
    {
        public static Error NotFound(Guid voucherId) => Error.NotFound(
            "Voucher.NotFound",
            $"Voucher '{voucherId}' was not found.");

        public static readonly Error NotActive = Error.Validation(
            "Voucher.NotActive",
            "Voucher is not active in the current time window.");

        public static readonly Error InvalidValue = Error.Validation(
            "Voucher.InvalidValue",
            "Voucher value is invalid.");

        public static readonly Error InvalidDateRange = Error.Validation(
            "Voucher.InvalidDateRange",
            "Voucher date range is invalid.");
    }
}