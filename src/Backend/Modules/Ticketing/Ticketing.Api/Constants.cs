using Microsoft.Extensions.Logging;

namespace Ticketing.Api;

internal static class Constants
{
    internal static class Tags
    {
        public const string Orders = "Ticketing - Orders";
        public const string Vouchers = "Ticketing - Vouchers";
        public const string Reports = "Ticketing - Report";
    }

    internal static class Routes
    {
        private const string BaseApi = "api/ticketing";

        public const string Orders = $"{BaseApi}/orders";
        public const string OrderById = $"{BaseApi}/{{orderId:guid}}";
        public const string ApplyVoucher = $"{BaseApi}/{{orderId:guid}}/apply-voucher";
        public const string CancelOrder = $"{Orders}/{{orderId:guid}}/cancel";
        public const string MyOrders = $"{Orders}/me";

        public const string CheckIn = $"{BaseApi}/events/{{eventId:guid}}/check-in";
        public const string GetTicketsForCheckIn = $"{BaseApi}/events/{{eventId:guid}}/check-in/tickets";
        public const string ManualCheckIn = $"{BaseApi}/events/{{eventId:guid}}/check-in/manual";

        // voucher
        public const string Vouchers = $"{BaseApi}/vouchers";
        public const string VoucherById = $"{Vouchers}/{{voucherId:guid}}";


        // organizer routes
        public const string OrganizerApi = $"api/organizer/ticketing";
        public const string OrganizerOrdersForEvent = $"{OrganizerApi}/orders";
        public const string ExportOrdersSheet = $"{OrganizerApi}/orders/export";
        public const string ExportVouchersSheet = $"{OrganizerApi}/vouchers/export";

        // reports
        public const string CheckInStats = $"{BaseApi}/report/events/{{eventId:guid}}/check-in-stats";
        public const string EventTicketSales = $"{BaseApi}/report/events/{{eventId:guid}}/ticket-sales";

    }
}
