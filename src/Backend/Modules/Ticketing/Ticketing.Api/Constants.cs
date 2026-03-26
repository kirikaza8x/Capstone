namespace Ticketing.Api;

internal static class Constants
{
    internal static class Tags
    {
        public const string Orders = "Ticketing - Orders";
        public const string Vouchers = "Ticketing - Vouchers";
    }

    internal static class Routes
    {
        private const string BaseApi = "api/ticketing";

        public const string Orders = $"{BaseApi}/orders";
        public const string OrderById = $"{BaseApi}/{{orderId:guid}}";
        public const string CheckIn = $"{BaseApi}/check-in";
        public const string ApplyVoucher = $"{BaseApi}/{{orderId:guid}}/apply-voucher";
        public const string CancelOrder = $"{Orders}/{{orderId:guid}}/cancel";

        // voucher
        public const string Vouchers = $"{BaseApi}/vouchers";
        public const string VoucherById = $"{Vouchers}/{{voucherId:guid}}";


        // organizer routes
        public const string OrganizerApi = $"api/organizer/ticketing";
        public const string OrganizerOrdersForEvent = $"{OrganizerApi}/orders";
        public const string ExportOrdersSheet = $"{OrganizerApi}/orders/export";
        public const string ExportVouchersSheet = $"{OrganizerApi}/vouchers/export";
        public const string MyOrders = $"{OrganizerApi}/orders/me";

    }
}
