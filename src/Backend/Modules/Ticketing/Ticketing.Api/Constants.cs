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
        public const string MyOrders = $"{BaseApi}/orders/me";
        public const string CancelOrder = $"{Orders}/{{orderId:guid}}/cancel";

        // voucher
        public const string Vouchers = $"{BaseApi}/vouchers";
        public const string VoucherById = $"{Vouchers}/{{voucherId:guid}}";
    }
}
